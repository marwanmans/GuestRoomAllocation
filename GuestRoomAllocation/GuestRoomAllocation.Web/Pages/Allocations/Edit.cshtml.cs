using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Allocations
{
    [Authorize]
    public class EditAllocationModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditAllocationModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Allocation Allocation { get; set; } = new();

        public List<Guest> Guests { get; set; } = new();
        public List<Apartment> Apartments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Allocation = await _context.Allocations
                .Include(a => a.Guest)
                .Include(a => a.Room)
                    .ThenInclude(r => r.Apartment)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (Allocation == null)
            {
                return NotFound();
            }

            await LoadDataAsync();
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

            // Check for room availability conflicts (excluding current allocation)
            var conflictingAllocations = await _context.Allocations
                .Where(a => a.RoomId == Allocation.RoomId &&
                           a.Id != Allocation.Id &&
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

            _context.Attach(Allocation).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await AllocationExists(Allocation.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            TempData["SuccessMessage"] = "Allocation updated successfully.";
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

        public string GetAllocationStatus()
        {
            var today = DateTime.Today;

            if (Allocation.CheckInDate <= today && Allocation.CheckOutDate > today)
                return "current";
            else if (Allocation.CheckInDate > today)
                return "upcoming";
            else
                return "past";
        }

        private async Task<bool> AllocationExists(int id)
        {
            return await _context.Allocations.AnyAsync(e => e.Id == id);
        }
    }
}