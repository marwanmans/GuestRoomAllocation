using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;
using GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace GuestRoomAllocation.Web.Pages.Allocations
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CreateAllocationCommand Command { get; set; } = default!;

        public SelectList GuestOptions { get; set; } = default!;
        public SelectList RoomOptions { get; set; } = default!;
        public IList<Room> AvailableRooms { get; set; } = new List<Room>();

        public async Task<IActionResult> OnGetAsync(int? guestId = null, int? roomId = null)
        {
            await LoadOptions();

            Command = new CreateAllocationCommand();

            // Pre-populate if parameters provided
            if (guestId.HasValue)
            {
                Command.GuestId = guestId.Value;
            }
            if (roomId.HasValue)
            {
                Command.RoomId = roomId.Value;
            }

            // Set default dates
            Command.StartDate = DateTime.Today.AddDays(1);
            Command.EndDate = DateTime.Today.AddDays(2);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return Page();
            }

            try
            {
                // Validate guest exists
                var guest = await _context.Guests
                    .FirstOrDefaultAsync(g => g.Id == Command.GuestId);
                if (guest == null)
                {
                    ModelState.AddModelError("Command.GuestId", "Selected guest does not exist.");
                    await LoadOptions();
                    return Page();
                }

                // Validate room exists
                var room = await _context.Rooms
                    .Include(r => r.Apartment)
                    .Include(r => r.Allocations)
                    .FirstOrDefaultAsync(r => r.Id == Command.RoomId);
                if (room == null)
                {
                    ModelState.AddModelError("Command.RoomId", "Selected room does not exist.");
                    await LoadOptions();
                    return Page();
                }

                // Create date range
                var dateRange = new DateRange(Command.StartDate, Command.EndDate);

                // Check room availability
                if (!room.IsAvailable(dateRange))
                {
                    // Find conflicting allocations
                    var conflicts = room.Allocations
                        .Where(a => a.DateRange.Overlaps(dateRange) &&
                                   a.Status != Domain.Enums.AllocationStatus.Cancelled)
                        .ToList();

                    if (conflicts.Any())
                    {
                        var conflictDetails = string.Join(", ", conflicts.Select(c =>
                            $"{c.Guest.FirstName} {c.Guest.LastName} ({c.DateRange.StartDate:MMM dd} - {c.DateRange.EndDate:MMM dd})"));
                        ModelState.AddModelError("", $"Room {room.RoomNumber} is not available for the selected dates. Conflicts with: {conflictDetails}");
                    }
                    else
                    {
                        ModelState.AddModelError("", $"Room {room.RoomNumber} is not available for the selected dates.");
                    }

                    await LoadOptions();
                    return Page();
                }

                // Check for guest conflicts (same guest, overlapping dates)
                var guestConflicts = await _context.Allocations
                    .Where(a => a.GuestId == Command.GuestId &&
                               a.Status != Domain.Enums.AllocationStatus.Cancelled)
                    .ToListAsync();

                var overlappingGuestAllocations = guestConflicts
                    .Where(a => a.DateRange.Overlaps(dateRange))
                    .ToList();

                if (overlappingGuestAllocations.Any())
                {
                    var conflict = overlappingGuestAllocations.First();
                    ModelState.AddModelError("",
                        $"{guest.FirstName} {guest.LastName} already has an allocation from {conflict.DateRange.StartDate:MMM dd} to {conflict.DateRange.EndDate:MMM dd} in {conflict.Room.Apartment.Name} - Room {conflict.Room.RoomNumber}.");
                    await LoadOptions();
                    return Page();
                }

                // Create allocation
                var allocation = new Allocation(
                    Command.GuestId,
                    Command.RoomId,
                    dateRange,
                    Command.BathroomPreferenceOverride,
                    Command.Notes
                );

                _context.Allocations.Add(allocation);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Allocation created successfully! {guest.FirstName} {guest.LastName} is assigned to {room.Apartment.Name} - Room {room.RoomNumber} from {dateRange.StartDate:MMM dd} to {dateRange.EndDate:MMM dd, yyyy}.";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while creating the allocation: {ex.Message}");
                await LoadOptions();
                return Page();
            }
        }

        public async Task<IActionResult> OnGetCheckAvailabilityAsync(DateTime startDate, DateTime endDate)
        {
            if (startDate >= endDate)
            {
                return new JsonResult(new { available = false, message = "End date must be after start date." });
            }

            try
            {
                var dateRange = new DateRange(startDate, endDate);

                var availableRooms = await _context.Rooms
                    .Include(r => r.Apartment)
                    .Include(r => r.Allocations.Where(a => a.Status != Domain.Enums.AllocationStatus.Cancelled))
                    .ToListAsync();

                var filteredRooms = availableRooms
                    .Where(r => r.IsAvailable(dateRange))
                    .Select(r => new
                    {
                        id = r.Id,
                        text = $"{r.Apartment.Name} - Room {r.RoomNumber} ({r.Size} sq ft, Max: {r.MaxOccupancy})",
                        apartment = r.Apartment.Name,
                        roomNumber = r.RoomNumber,
                        size = r.Size,
                        maxOccupancy = r.MaxOccupancy,
                        hasPrivateBathroom = r.HasPrivateBathroom
                    })
                    .OrderBy(r => r.apartment)
                    .ThenBy(r => r.roomNumber)
                    .ToList();

                return new JsonResult(new { available = true, rooms = filteredRooms });
            }
            catch
            {
                return new JsonResult(new { available = false, message = "Error checking availability." });
            }
        }

        private async Task LoadOptions()
        {
            var guests = await _context.Guests
                .OrderBy(g => g.FirstName)
                .ThenBy(g => g.LastName)
                .Select(g => new {
                    g.Id,
                    Name = $"{g.FirstName} {g.LastName} ({g.ContactInfo.Email})"
                })
                .ToListAsync();

            var rooms = await _context.Rooms
                .Include(r => r.Apartment)
                .OrderBy(r => r.Apartment.Name)
                .ThenBy(r => r.RoomNumber)
                .Select(r => new {
                    r.Id,
                    Name = $"{r.Apartment.Name} - Room {r.RoomNumber} ({r.Size} sq ft)"
                })
                .ToListAsync();

            GuestOptions = new SelectList(guests, "Id", "Name");
            RoomOptions = new SelectList(rooms, "Id", "Name");
        }
    }

    public class CreateAllocationCommand
    {
        [Required]
        [Display(Name = "Guest")]
        public int GuestId { get; set; }

        [Required]
        [Display(Name = "Room")]
        public int RoomId { get; set; }

        [Required]
        [Display(Name = "Check-in Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "Check-out Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Override Bathroom Preference")]
        public bool BathroomPreferenceOverride { get; set; }

        [StringLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        // Custom validation
        public bool IsValidDateRange => EndDate > StartDate;
        public bool IsValidStartDate => StartDate >= DateTime.Today;
        public int Duration => IsValidDateRange ? (EndDate - StartDate).Days : 0;
    }
}