
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Rooms
{
    [Authorize]
    public class EditRoomModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditRoomModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Room Room { get; set; } = new();

        public List<Apartment> Apartments { get; set; } = new();
        public Allocation? CurrentAllocation { get; set; }
        public Allocation? UpcomingAllocation { get; set; }
        public Allocation? LastAllocation { get; set; }
        public MaintenancePeriod? CurrentMaintenance { get; set; }
        public List<Allocation> RecentAllocations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Room = await _context.Rooms
                .Include(r => r.Apartment)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (Room == null)
            {
                return NotFound();
            }

            await LoadDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadDataAsync();
                return Page();
            }

            // Check for duplicate room number within the same apartment (excluding current room)
            var existingRoom = await _context.Rooms
                .FirstOrDefaultAsync(r => r.ApartmentId == Room.ApartmentId &&
                                         r.RoomNumber == Room.RoomNumber &&
                                         r.Id != Room.Id);

            if (existingRoom != null)
            {
                ModelState.AddModelError("Room.RoomNumber", "A room with this number already exists in the selected apartment.");
                await LoadDataAsync();
                return Page();
            }

            // Validate room size
            if (Room.Size <= 0)
            {
                ModelState.AddModelError("Room.Size", "Room size must be greater than 0.");
                await LoadDataAsync();
                return Page();
            }

            // Validate max occupancy
            if (Room.MaxOccupancy <= 0 || Room.MaxOccupancy > 4)
            {
                ModelState.AddModelError("Room.MaxOccupancy", "Max occupancy must be between 1 and 4.");
                await LoadDataAsync();
                return Page();
            }

            _context.Attach(Room).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await RoomExists(Room.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            TempData["SuccessMessage"] = $"Room {Room.RoomNumber} has been updated successfully.";
            return RedirectToPage("./Index");
        }

        private async Task LoadDataAsync()
        {
            var today = DateTime.Today;

            Apartments = await _context.Apartments
                .OrderBy(a => a.Name)
                .ToListAsync();

            // Load current allocation
            CurrentAllocation = await _context.Allocations
                .Include(a => a.Guest)
                .FirstOrDefaultAsync(a => a.RoomId == Room.Id &&
                                         a.CheckInDate <= today &&
                                         a.CheckOutDate > today);

            // Load upcoming allocation
            UpcomingAllocation = await _context.Allocations
                .Include(a => a.Guest)
                .Where(a => a.RoomId == Room.Id && a.CheckInDate > today)
                .OrderBy(a => a.CheckInDate)
                .FirstOrDefaultAsync();

            // Load last allocation
            LastAllocation = await _context.Allocations
                .Include(a => a.Guest)
                .Where(a => a.RoomId == Room.Id && a.CheckOutDate <= today)
                .OrderByDescending(a => a.CheckOutDate)
                .FirstOrDefaultAsync();

            // Load current maintenance
            CurrentMaintenance = await _context.MaintenancePeriods
                .FirstOrDefaultAsync(m => m.RoomId == Room.Id &&
                                         m.StartDate <= today &&
                                         m.EndDate >= today);

            // Load recent allocations (last 6 months)
            var sixMonthsAgo = today.AddMonths(-6);
            RecentAllocations = await _context.Allocations
                .Include(a => a.Guest)
                .Where(a => a.RoomId == Room.Id && a.CheckInDate >= sixMonthsAgo)
                .OrderByDescending(a => a.CheckInDate)
                .ToListAsync();
        }

        private async Task<bool> RoomExists(int id)
        {
            return await _context.Rooms.AnyAsync(e => e.Id == id);
        }
    }
}