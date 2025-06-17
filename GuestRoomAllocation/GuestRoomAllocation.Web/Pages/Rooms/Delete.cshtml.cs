using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Rooms
{
    [Authorize]
    public class DeleteRoomModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteRoomModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Room Room { get; set; } = new();

        public int AllocationCount { get; set; }
        public int CurrentAllocationCount { get; set; }
        public int FutureAllocationCount { get; set; }
        public int MaintenanceCount { get; set; }
        public List<Allocation> RecentAllocations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Room = await _context.Rooms
                .Include(r => r.Apartment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (Room == null)
            {
                return NotFound();
            }

            var today = DateTime.Today;
            var allocations = await _context.Allocations
                .Include(a => a.Guest)
                .Where(a => a.RoomId == id)
                .ToListAsync();

            AllocationCount = allocations.Count;
            CurrentAllocationCount = allocations.Count(a => a.CheckInDate <= today && a.CheckOutDate > today);
            FutureAllocationCount = allocations.Count(a => a.CheckInDate > today);

            MaintenanceCount = await _context.MaintenancePeriods
                .CountAsync(m => m.RoomId == id);

            RecentAllocations = allocations
                .OrderByDescending(a => a.CheckInDate)
                .Take(10)
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Room = await _context.Rooms
                .Include(r => r.Apartment)
                .FirstOrDefaultAsync(r => r.Id == Room.Id);

            if (Room == null)
            {
                return NotFound();
            }

            var roomInfo = $"{Room.Apartment.Name} - Room {Room.RoomNumber}";

            _context.Rooms.Remove(Room);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Room {roomInfo} has been deleted successfully.";
            return RedirectToPage("./Index");
        }
    }
}