
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Admin
{
    [Authorize]
    public class CreateApartmentModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateApartmentModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Apartment Apartment { get; set; } = new();

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

            _context.Apartments.Add(Apartment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Apartment '{Apartment.Name}' has been created successfully. You can now add rooms to it.";
            return RedirectToPage("./Apartments");
        }
    }
}