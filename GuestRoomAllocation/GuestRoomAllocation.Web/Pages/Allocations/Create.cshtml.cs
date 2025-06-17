using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Allocations
{
    [Authorize]
    public class CreateAllocationModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateAllocationModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Allocation Allocation { get; set; } = new()
        {
            CheckInDate = DateTime.Today.AddDays(1),
            CheckOutDate = DateTime.Today.AddDays(8)
        };

        public List<Guest> Guests { get; set; } = new();
        public List<Apartment> Apartments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? guestId = null, int? roomId = null, DateTime? checkInDate = null, DateTime? checkOutDate = null)
        {
            await LoadDataAsync();

            if (guestId.HasValue)
            {
                Allocation.GuestId = guestId.Value;
            }

            if (roomId.HasValue)
            {
                Allocation.RoomId = roomId.Value;
            }

            // Handle date parameters from calendar clicks
            if (checkInDate.HasValue)
            {
                Allocation.CheckInDate = checkInDate.Value;

                // If check-out date not provided, default to next day
                if (!checkOutDate.HasValue)
                {
                    Allocation.CheckOutDate = checkInDate.Value.AddDays(1);
                }
            }

            if (checkOutDate.HasValue)
            {
                Allocation.CheckOutDate = checkOutDate.Value;
            }

            // Validate that check-out is after check-in
            if (Allocation.CheckOutDate <= Allocation.CheckInDate)
            {
                Allocation.CheckOutDate = Allocation.CheckInDate.AddDays(1);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            // Validate dates
            if (Allocation.CheckInDate >= Allocation.CheckOutDate)
            {
                ModelState.AddModelError("Allocation.CheckOutDate", "Check-out date must be after check-in date.");
                await LoadDataAsync();
                return Page();
            }

            // Check for room availability conflicts
            var conflictingAllocations = await _context.Allocations
                .Where(a => a.RoomId == Allocation.RoomId &&
                           a.CheckInDate < Allocation.CheckOutDate &&
                           a.CheckOutDate > Allocation.CheckInDate)
                .CountAsync();

            if (conflictingAllocations > 0)
            {
                ModelState.AddModelError("Allocation.RoomId", "This room is not available for the selected dates.");
                await LoadDataAsync();
                return Page();
            }

            // Check for maintenance periods
            var maintenanceConflict = await _context.MaintenancePeriods
                .Where(m => m.RoomId == Allocation.RoomId &&
                           m.StartDate < Allocation.CheckOutDate &&
                           m.EndDate >= Allocation.CheckInDate)
                .AnyAsync();

            if (maintenanceConflict)
            {
                ModelState.AddModelError("Allocation.RoomId", "This room has scheduled maintenance during the selected dates.");
                await LoadDataAsync();
                return Page();
            }

            // Check bathroom preference if not overridden
            if (!Allocation.BathroomPreferenceOverride)
            {
                var room = await _context.Rooms
                    .Include(r => r.Apartment)
                    .FirstOrDefaultAsync(r => r.Id == Allocation.RoomId);

                if (room != null && !room.HasPrivateBathroom)
                {
                    // Check if bathroom would be overcrowded
                    var apartmentAllocations = await _context.Allocations
                        .Include(a => a.Room)
                        .Where(a => a.Room.ApartmentId == room.ApartmentId &&
                                   a.CheckInDate < Allocation.CheckOutDate &&
                                   a.CheckOutDate > Allocation.CheckInDate)
                        .ToListAsync();

                    var guestsPerBathroom = (double)(apartmentAllocations.Count + 1) / room.Apartment.TotalBathrooms;

                    if (guestsPerBathroom > 1.5) // Allow some flexibility
                    {
                        TempData["WarningMessage"] = "This allocation may overcrowd the bathrooms. Consider using bathroom override or selecting a different room.";
                    }
                }
            }

            _context.Allocations.Add(Allocation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Room allocation created successfully.";
            return RedirectToPage("./Index");
        }

        private async Task LoadDataAsync()
        {
            Guests = await _context.Guests
                .OrderBy(g => g.LastName)
                .ThenBy(g => g.FirstName)
                .ToListAsync();

            Apartments = await _context.Apartments
                .Include(a => a.Rooms)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public bool IsRoomAvailable(int roomId, DateTime checkIn, DateTime checkOut, int excludeAllocationId = 0)
        {
            var conflictingAllocations = _context.Allocations
                .Where(a => a.RoomId == roomId &&
                           a.Id != excludeAllocationId &&
                           a.CheckInDate < checkOut &&
                           a.CheckOutDate > checkIn)
                .Count();

            var maintenanceConflict = _context.MaintenancePeriods
                .Where(m => m.RoomId == roomId &&
                           m.StartDate < checkOut &&
                           m.EndDate >= checkIn)
                .Any();

            return conflictingAllocations == 0 && !maintenanceConflict;
        }
    }
}