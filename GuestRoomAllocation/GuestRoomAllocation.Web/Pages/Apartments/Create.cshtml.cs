using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GuestRoomAllocation.Persistence;
using GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace GuestRoomAllocation.Web.Pages.Apartments
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CreateApartmentCommand Command { get; set; } = new();

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

            try
            {
                var address = new Address(Command.Street, Command.City, Command.Country);

                var apartment = new Apartment(Command.Name, address, Command.TotalBathrooms, Command.OverallSpace);

                apartment.UpdateDetails(
                    Command.Name,
                    address,
                    Command.TotalBathrooms,
                    Command.OverallSpace,
                    Command.MapLocation,
                    Command.CommonAreas,
                    Command.Facilities,
                    Command.Amenities,
                    Command.HasLaundry);

                _context.Apartments.Add(apartment);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Apartment '{Command.Name}' has been created successfully!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while creating the apartment: {ex.Message}");
                return Page();
            }
        }
    }

    public class CreateApartmentCommand
    {
        [Required]
        [StringLength(100)]
        [Display(Name = "Apartment Name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Street Address")]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "City")]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Display(Name = "Country")]
        public string Country { get; set; } = "Egypt";

        [StringLength(500)]
        [Display(Name = "Map Location")]
        public string? MapLocation { get; set; }

        [Required]
        [Range(1, 50)]
        [Display(Name = "Total Bathrooms")]
        public int TotalBathrooms { get; set; } = 1;

        [Required]
        [Range(20, 1000)]
        [Display(Name = "Overall Space (m²)")]
        public int OverallSpace { get; set; } = 50;

        [StringLength(1000)]
        [Display(Name = "Common Areas")]
        public string? CommonAreas { get; set; }

        [StringLength(1000)]
        [Display(Name = "Facilities")]
        public string? Facilities { get; set; }

        [StringLength(1000)]
        [Display(Name = "Amenities")]
        public string? Amenities { get; set; }

        [Display(Name = "Has Laundry")]
        public bool HasLaundry { get; set; }
    }
}