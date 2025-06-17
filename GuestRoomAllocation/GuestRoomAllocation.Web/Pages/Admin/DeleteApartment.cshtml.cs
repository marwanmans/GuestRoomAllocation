
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Admin
{
    [Authorize]
    public class DeleteApartmentModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteApartmentModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Apartment Apartment { get; set; } = new();

        public int TotalRooms { get; set; }
        public int CurrentAllocations { get; set; }
        public int FutureAllocations { get; set; }
        public int PastAllocations { get; set; }
        public int MaintenancePeriods { get; set; }

        public List<Allocation> ActiveAllocations { get; set; } = new();
        public List<Allocation> UpcomingAllocations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Apartment = await _context.Apartments
                .Include(a => a.Rooms)
                .Include(a => a.MaintenancePeriods)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (Apartment == null)
            {
                return NotFound();
            }

            await LoadImpactDataAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Apartment = await _context.Apartments
                .Include(a => a.Rooms)
                .FirstOrDefaultAsync(a => a.Id == Apartment.Id);

            if (Apartment == null)
            {
                return NotFound();
            }

            // Reload impact data to check for active allocations
            await LoadImpactDataAsync();

            // Prevent deletion if there are current or future allocations
            if (CurrentAllocations > 0 || FutureAllocations > 0)
            {
                TempData["ErrorMessage"] = $"Cannot delete apartment '{Apartment.Name}' because it has active or future allocations. Please cancel or move these allocations first.";
                return Page();
            }

            var apartmentName = Apartment.Name;
            var roomCount = TotalRooms;

            // Delete the apartment (cascade delete will handle rooms, allocations, maintenance)
            _context.Apartments.Remove(Apartment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Apartment '{apartmentName}' and all its {roomCount} rooms have been deleted successfully.";
            TempData["InfoMessage"] = $"All historical data, allocations, and maintenance records for this apartment have been permanently removed.";

            return RedirectToPage("./Apartments");
        }

        private async Task LoadImpactDataAsync()
        {
            var today = DateTime.Today;
            var roomIds = Apartment.Rooms.Select(r => r.Id).ToList();

            TotalRooms = Apartment.Rooms.Count;

            // Load all allocations for this apartment's rooms
            var allAllocations = await _context.Allocations
                .Include(a => a.Guest)
                .Include(a => a.Room)
                .Where(a => roomIds.Contains(a.RoomId))
                .ToListAsync();

            // Categorize allocations by status
            ActiveAllocations = allAllocations
                .Where(a => a.CheckInDate <= today && a.CheckOutDate > today)
                .OrderBy(a => a.Room.RoomNumber)
                .ToList();

            UpcomingAllocations = allAllocations
                .Where(a => a.CheckInDate > today)
                .OrderBy(a => a.CheckInDate)
                .ToList();

            var pastAllocations = allAllocations
                .Where(a => a.CheckOutDate <= today)
                .ToList();

            // Set counts
            CurrentAllocations = ActiveAllocations.Count;
            FutureAllocations = UpcomingAllocations.Count;
            PastAllocations = pastAllocations.Count;

            // Load maintenance periods
            MaintenancePeriods = await _context.MaintenancePeriods
                .Where(m => m.ApartmentId == Apartment.Id ||
                           (m.RoomId.HasValue && roomIds.Contains(m.RoomId.Value)))
                .CountAsync();
        }
    }
}