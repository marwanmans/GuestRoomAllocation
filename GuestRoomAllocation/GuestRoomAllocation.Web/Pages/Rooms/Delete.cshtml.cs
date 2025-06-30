using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;
using GuestRoomAllocation.Domain.Entities;

namespace GuestRoomAllocation.Web.Pages.Rooms
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Room Room { get; set; } = default!;

        public string ApartmentName { get; set; } = string.Empty;
        public bool HasActiveAllocations { get; set; }
        public bool HasFutureAllocations { get; set; }
        public int TotalAllocations { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Apartment)
                .Include(r => r.Allocations)
                    .ThenInclude(a => a.Guest)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            Room = room;
            ApartmentName = room.Apartment.Name;

            // Check for allocations
            var today = DateTime.Today;
            HasActiveAllocations = room.Allocations.Any(a =>
                a.DateRange.StartDate <= today && a.DateRange.EndDate >= today);

            HasFutureAllocations = room.Allocations.Any(a =>
                a.DateRange.StartDate > today);

            TotalAllocations = room.Allocations.Count;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var room = await _context.Rooms
                .Include(r => r.Apartment)
                .Include(r => r.Allocations)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (room == null)
            {
                return NotFound();
            }

            try
            {
                // Check for active allocations before deletion
                var today = DateTime.Today;
                var activeAllocations = room.Allocations
                    .Where(a => a.DateRange.StartDate <= today && a.DateRange.EndDate >= today)
                    .ToList();

                if (activeAllocations.Any())
                {
                    TempData["Error"] = $"Cannot delete room {room.RoomNumber}. There are currently {activeAllocations.Count} active allocation(s). Please end all current stays before deleting this room.";
                    return RedirectToPage("./Index");
                }

                // Check for future allocations and warn user
                var futureAllocations = room.Allocations
                    .Where(a => a.DateRange.StartDate > today)
                    .ToList();

                if (futureAllocations.Any())
                {
                    // Cancel future allocations
                    foreach (var allocation in futureAllocations)
                    {
                        allocation.Cancel();
                    }

                    TempData["Warning"] = $"Room {room.RoomNumber} deleted successfully. {futureAllocations.Count} future allocation(s) were automatically cancelled.";
                }
                else
                {
                    TempData["Success"] = $"Room {room.RoomNumber} deleted successfully from {room.Apartment.Name}.";
                }

                // Use the apartment's RemoveRoom method to maintain domain consistency
                room.Apartment.RemoveRoom(room);

                await _context.SaveChangesAsync();

                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"An error occurred while deleting the room: {ex.Message}";
                return RedirectToPage("./Index");
            }
        }
    }
}