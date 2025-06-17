
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Rooms
{
    [Authorize]
    public class CreateRoomModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateRoomModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Room Room { get; set; } = new();

        public List<Apartment> Apartments { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? apartmentId = null)
        {
            Apartments = await _context.Apartments
                .OrderBy(a => a.Name)
                .ToListAsync();

            if (apartmentId.HasValue)
            {
                Room.ApartmentId = apartmentId.Value;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                Apartments = await _context.Apartments
                    .OrderBy(a => a.Name)
                    .ToListAsync();
                return Page();
            }

            // Check for duplicate room number within the same apartment
            var existingRoom = await _context.Rooms
                .FirstOrDefaultAsync(r => r.ApartmentId == Room.ApartmentId &&
                                         r.RoomNumber == Room.RoomNumber);

            if (existingRoom != null)
            {
                ModelState.AddModelError("Room.RoomNumber", "A room with this number already exists in the selected apartment.");
                Apartments = await _context.Apartments
                    .OrderBy(a => a.Name)
                    .ToListAsync();
                return Page();
            }

            // Validate room size
            if (Room.Size <= 0)
            {
                ModelState.AddModelError("Room.Size", "Room size must be greater than 0.");
                Apartments = await _context.Apartments
                    .OrderBy(a => a.Name)
                    .ToListAsync();
                return Page();
            }

            // Validate max occupancy
            if (Room.MaxOccupancy <= 0 || Room.MaxOccupancy > 4)
            {
                ModelState.AddModelError("Room.MaxOccupancy", "Max occupancy must be between 1 and 4.");
                Apartments = await _context.Apartments
                    .OrderBy(a => a.Name)
                    .ToListAsync();
                return Page();
            }

            _context.Rooms.Add(Room);
            await _context.SaveChangesAsync();

            var apartment = await _context.Apartments.FindAsync(Room.ApartmentId);
            TempData["SuccessMessage"] = $"Room {Room.RoomNumber} has been added to {apartment?.Name}.";
            return RedirectToPage("./Index");
        }
    }
}