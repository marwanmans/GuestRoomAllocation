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
        public CreateApartmentCommand Command { get; set; } = default!;

        public IActionResult OnGet()
        {
            Command = new CreateApartmentCommand();
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
                var address = new Address(
                    Command.Street,
                    Command.City,
                    Command.State,
                    Command.ZipCode,
                    Command.Country
                );

                var apartment = new Apartment(
                    Command.Name,
                    address,
                    Command.TotalBathrooms,
                    Command.OverallSpace
                );

                // Set optional properties directly (assuming they are public properties)
                apartment.MapLocation = Command.MapLocation;
                apartment.CommonAreas = Command.CommonAreas;
                apartment.Facilities = Command.Facilities;
                apartment.Amenities = Command.Amenities;
                apartment.HasLaundry = Command.HasLaundry;

                _context.Apartments.Add(apartment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Apartment created successfully!";
                return RedirectToPage("./Index");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An error occurred while creating the apartment.");
                return Page();
            }
        }
    }

    public class CreateApartmentCommand
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        [Display(Name = "Street Address")]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        [Display(Name = "Zip Code")]
        public string ZipCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Map Location")]
        public string? MapLocation { get; set; }

        [Range(1, 50)]
        [Display(Name = "Total Bathrooms")]
        public int TotalBathrooms { get; set; } = 1;

        [StringLength(1000)]
        [Display(Name = "Common Areas")]
        public string? CommonAreas { get; set; }

        [StringLength(1000)]
        public string? Facilities { get; set; }

        [StringLength(1000)]
        public string? Amenities { get; set; }

        [Display(Name = "Has Laundry")]
        public bool HasLaundry { get; set; }

        [Range(100, 10000)]
        [Display(Name = "Overall Space (sq ft)")]
        public int OverallSpace { get; set; } = 1000;
    }
}