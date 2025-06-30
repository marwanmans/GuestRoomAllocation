using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;

namespace GuestRoomAllocation.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalGuests { get; set; }
        public int TotalApartments { get; set; }
        public int AvailableRooms { get; set; }
        public int ActiveAllocations { get; set; }
        public int PendingMaintenance { get; set; }

        public async Task OnGetAsync()
        {
            try
            {
                // Get real counts from database
                TotalGuests = await _context.Guests.CountAsync();
                TotalApartments = await _context.Apartments.CountAsync();

                // Total rooms (we'll calculate available later when we have occupancy logic)
                var totalRooms = await _context.Rooms.CountAsync();
                AvailableRooms = totalRooms; // For now, assume all rooms are available

                // Active allocations (current date between start and end date)
                // Fixed: Using DateRange property instead of Period
                ActiveAllocations = await _context.Allocations
                    .Where(a => a.DateRange.StartDate <= DateTime.Now && a.DateRange.EndDate >= DateTime.Now)
                    .CountAsync();

                // Pending maintenance
                PendingMaintenance = await _context.MaintenancePeriods
                    .Where(m => m.Status == 0) // Assuming 0 = Pending
                    .CountAsync();
            }
            catch (Exception ex)
            {
                // Fallback values if database query fails
                TotalGuests = 0;
                TotalApartments = 0;
                AvailableRooms = 0;
                ActiveAllocations = 0;
                PendingMaintenance = 0;

                // Log the error (optional)
                Console.WriteLine($"Database error: {ex.Message}");
            }
        }
    }
}