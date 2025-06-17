
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Admin
{
    [Authorize]
    public class EditApartmentModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditApartmentModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Apartment Apartment { get; set; } = new();

        public List<Allocation> CurrentAllocations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Apartment = await _context.Apartments
                .Include(a => a.Rooms)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (Apartment == null)
            {
                return NotFound();
            }

            // Load current allocations for this apartment
            var today = DateTime.Today;
            CurrentAllocations = await _context.Allocations
                .Include(a => a.Guest)
                .Include(a => a.Room)
                .Where(a => a.Room.ApartmentId == id &&
                           a.CheckInDate <= today &&
                           a.CheckOutDate > today)
                .OrderBy(a => a.Room.RoomNumber)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Reload data for display
                Apartment = await _context.Apartments
                    .Include(a => a.Rooms)
                    .FirstOrDefaultAsync(a => a.Id == Apartment.Id);

                var today = DateTime.Today;
                CurrentAllocations = await _context.Allocations
                    .Include(a => a.Guest)
                    .Include(a => a.Room)
                    .Where(a => a.Room.ApartmentId == Apartment.Id &&
                               a.CheckInDate <= today &&
                               a.CheckOutDate > today)
                    .OrderBy(a => a.Room.RoomNumber)
                    .ToListAsync();

                return Page();
            }

            // Validate Map Location URL if provided
            if (!string.IsNullOrEmpty(Apartment.MapLocation))
            {
                if (!Uri.TryCreate(Apartment.MapLocation, UriKind.Absolute, out var uri) ||
                    (!uri.Host.Contains("google.com") && !uri.Host.Contains("maps.google")))
                {
                    ModelState.AddModelError("Apartment.MapLocation", "Please provide a valid Google Maps URL.");
                    return Page();
                }
            }

            _context.Attach(Apartment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ApartmentExists(Apartment.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            TempData["SuccessMessage"] = $"Apartment '{Apartment.Name}' has been updated successfully.";
            return RedirectToPage("./Apartments");
        }

        private async Task<bool> ApartmentExists(int id)
        {
            return await _context.Apartments.AnyAsync(e => e.Id == id);
        }
    }
}