
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GuestRoomAllocation.Web.Pages.Guests
{
    [Authorize]
    public class CreateGuestModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateGuestModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Guest Guest { get; set; } = new();

        public IActionResult OnGet()
        {
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check for duplicate email
            var existingGuest = await _context.Guests
                .FirstOrDefaultAsync(g => g.Email == Guest.Email);

            if (existingGuest != null)
            {
                ModelState.AddModelError("Guest.Email", "A guest with this email already exists.");
                return Page();
            }

            _context.Guests.Add(Guest);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Guest {Guest.FullName} has been created successfully.";
            return RedirectToPage("./Index");
        }
    }
}
