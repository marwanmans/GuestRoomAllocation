
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Allocations
{
    [Authorize]
    public class DeleteAllocationModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteAllocationModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Allocation Allocation { get; set; } = new();

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

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Allocation = await _context.Allocations
                .Include(a => a.Guest)
                .Include(a => a.Room)
                    .ThenInclude(r => r.Apartment)
                .FirstOrDefaultAsync(a => a.Id == Allocation.Id);

            if (Allocation == null)
            {
                return NotFound();
            }

            var guestName = Allocation.Guest.FullName;
            var roomInfo = $"{Allocation.Room.Apartment.Name} - {Allocation.Room.RoomNumber}";
            var status = GetAllocationStatus();

            _context.Allocations.Remove(Allocation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Allocation for {guestName} in {roomInfo} has been deleted successfully.";

            // Add additional message based on status
            if (status == "current")
            {
                TempData["InfoMessage"] = "The room is now available for immediate allocation.";
            }
            else if (status == "upcoming")
            {
                TempData["InfoMessage"] = $"The room is now available from {Allocation.CheckInDate:MMM dd, yyyy}.";
            }

            return RedirectToPage("./Index");
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
    }
}