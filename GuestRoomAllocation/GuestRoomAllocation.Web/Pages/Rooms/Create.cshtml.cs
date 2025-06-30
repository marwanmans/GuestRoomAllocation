using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;
using GuestRoomAllocation.Domain.Entities;
using System.ComponentModel.DataAnnotations;

namespace GuestRoomAllocation.Web.Pages.Rooms
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CreateRoomCommand Command { get; set; } = default!;

        public SelectList ApartmentOptions { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? apartmentId = null)
        {
            await LoadApartmentOptions();

            Command = new CreateRoomCommand();
            if (apartmentId.HasValue)
            {
                Command.ApartmentId = apartmentId.Value;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadApartmentOptions();
                return Page();
            }

            try
            {
                // Check if apartment exists
                var apartment = await _context.Apartments
                    .Include(a => a.Rooms)
                    .FirstOrDefaultAsync(a => a.Id == Command.ApartmentId);

                if (apartment == null)
                {
                    ModelState.AddModelError("Command.ApartmentId", "Selected apartment does not exist.");
                    await LoadApartmentOptions();
                    return Page();
                }

                // Check for duplicate room number in the same apartment
                if (apartment.Rooms.Any(r => r.RoomNumber == Command.RoomNumber))
                {
                    ModelState.AddModelError("Command.RoomNumber", $"Room {Command.RoomNumber} already exists in {apartment.Name}.");
                    await LoadApartmentOptions();
                    return Page();
                }

                // Create the room using the apartment's AddRoom method
                var room = apartment.AddRoom(
                    Command.RoomNumber,
                    Command.Size,
                    Command.HasPrivateBathroom,
                    Command.MaxOccupancy,
                    Command.Description
                );

                await _context.SaveChangesAsync();

                TempData["Success"] = $"Room {Command.RoomNumber} created successfully in {apartment.Name}!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while creating the room: {ex.Message}");
                await LoadApartmentOptions();
                return Page();
            }
        }

        private async Task LoadApartmentOptions()
        {
            var apartments = await _context.Apartments
                .OrderBy(a => a.Name)
                .Select(a => new { a.Id, a.Name })
                .ToListAsync();

            ApartmentOptions = new SelectList(apartments, "Id", "Name");
        }
    }

    public class CreateRoomCommand
    {
        [Required]
        [Display(Name = "Apartment")]
        public int ApartmentId { get; set; }

        [Required]
        [StringLength(10)]
        [Display(Name = "Room Number")]
        public string RoomNumber { get; set; } = string.Empty;

        [Required]
        [Range(50, 2000)]
        [Display(Name = "Size (sq ft)")]
        public int Size { get; set; } = 100;

        [Required]
        [Range(1, 6)]
        [Display(Name = "Maximum Occupancy")]
        public int MaxOccupancy { get; set; } = 1;

        [Display(Name = "Has Private Bathroom")]
        public bool HasPrivateBathroom { get; set; }

        [StringLength(500)]
        [Display(Name = "Description")]
        public string? Description { get; set; }
    }
}