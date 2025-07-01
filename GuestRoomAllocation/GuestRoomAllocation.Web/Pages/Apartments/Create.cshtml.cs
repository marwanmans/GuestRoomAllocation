using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
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

            // DEBUG: Add this debugging section
            try
            {
                var connectionString = _context.Database.GetDbConnection().ConnectionString;
                var databaseName = _context.Database.GetDbConnection().Database;

                // This will appear in the Output window
                System.Diagnostics.Debug.WriteLine($"Connection String: {connectionString}");
                System.Diagnostics.Debug.WriteLine($"Database Name: {databaseName}");

                // Test if we can see the Apartments table
                var canSeeTable = await _context.Database.CanConnectAsync();
                System.Diagnostics.Debug.WriteLine($"Can connect to database: {canSeeTable}");

                // Try to count apartments (this will fail if table doesn't exist)
                var apartmentCount = await _context.Apartments.CountAsync();
                System.Diagnostics.Debug.WriteLine($"Current apartment count: {apartmentCount}");
            }
            catch (Exception debugEx)
            {
                System.Diagnostics.Debug.WriteLine($"Debug error: {debugEx.Message}");
                ModelState.AddModelError("", $"Debug info: {debugEx.Message}");
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

                // Use UpdateDetails method to set optional properties
                apartment.UpdateDetails(
                    Command.Name,
                    address,
                    Command.TotalBathrooms,
                    Command.OverallSpace,
                    Command.MapLocation,
                    Command.CommonAreas,
                    Command.Facilities,
                    Command.Amenities,
                    Command.HasLaundry
                );

                _context.Apartments.Add(apartment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Apartment created successfully!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                // Show detailed error for debugging
                ModelState.AddModelError("", $"Error: {ex.Message}");
                if (ex.InnerException != null)
                {
                    ModelState.AddModelError("", $"Inner Error: {ex.InnerException.Message}");
                }
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