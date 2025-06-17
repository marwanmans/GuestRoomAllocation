
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Guests
{
    [Authorize]
    public class EditGuestModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditGuestModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Guest Guest { get; set; } = new();

        public List<Allocation> Allocations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Guest = await _context.Guests.FindAsync(id);
            if (Guest == null)
            {
                return NotFound();
            }

            Allocations = await _context.Allocations
                .Include(a => a.Room)
                    .ThenInclude(r => r.Apartment)
                .Where(a => a.GuestId == id)
                .OrderByDescending(a => a.CheckInDate)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload allocations for display
                Allocations = await _context.Allocations
                    .Include(a => a.Room)
                        .ThenInclude(r => r.Apartment)
                    .Where(a => a.GuestId == Guest.Id)
                    .OrderByDescending(a => a.CheckInDate)
                    .ToListAsync();
                return Page();
            }

            // Check for duplicate email (excluding current guest)
            var existingGuest = await _context.Guests
                .FirstOrDefaultAsync(g => g.Email == Guest.Email && g.Id != Guest.Id);

            if (existingGuest != null)
            {
                ModelState.AddModelError("Guest.Email", "A guest with this email already exists.");
                Allocations = await _context.Allocations
                    .Include(a => a.Room)
                        .ThenInclude(r => r.Apartment)
                    .Where(a => a.GuestId == Guest.Id)
                    .OrderByDescending(a => a.CheckInDate)
                    .ToListAsync();
                return Page();
            }

            _context.Attach(Guest).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await GuestExists(Guest.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            TempData["SuccessMessage"] = $"Guest {Guest.FullName} has been updated successfully.";
            return RedirectToPage("./Index");
        }

        private async Task<bool> GuestExists(int id)
        {
            return await _context.Guests.AnyAsync(e => e.Id == id);
        }
    }
}