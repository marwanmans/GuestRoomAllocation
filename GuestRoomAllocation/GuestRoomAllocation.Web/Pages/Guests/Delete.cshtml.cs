
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Guests
{
    [Authorize]
    public class DeleteGuestModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteGuestModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Guest Guest { get; set; } = new();

        public int AllocationCount { get; set; }
        public int CurrentAllocationCount { get; set; }
        public int FutureAllocationCount { get; set; }
        public List<Allocation> RecentAllocations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Guest = await _context.Guests.FindAsync(id);
            if (Guest == null)
            {
                return NotFound();
            }

            var today = DateTime.Today;
            var allocations = await _context.Allocations
                .Include(a => a.Room)
                    .ThenInclude(r => r.Apartment)
                .Where(a => a.GuestId == id)
                .ToListAsync();

            AllocationCount = allocations.Count;
            CurrentAllocationCount = allocations.Count(a => a.CheckInDate <= today && a.CheckOutDate > today);
            FutureAllocationCount = allocations.Count(a => a.CheckInDate > today);

            RecentAllocations = allocations
                .OrderByDescending(a => a.CheckInDate)
                .Take(5)
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Guest = await _context.Guests.FindAsync(Guest.Id);
            if (Guest == null)
            {
                return NotFound();
            }

            _context.Guests.Remove(Guest);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Guest {Guest.FullName} has been deleted successfully.";
            return RedirectToPage("./Index");
        }
    }
}