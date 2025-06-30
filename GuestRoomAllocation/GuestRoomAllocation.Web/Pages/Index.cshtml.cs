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
            // Get real counts from database
            TotalGuests = await _context.Guests.CountAsync();
            TotalApartments = await _context.Apartments.CountAsync();

            // Total rooms (we'll calculate available later when we have occupancy logic)
            var totalRooms = await _context.Rooms.CountAsync();
            AvailableRooms = totalRooms; // For now, assume all rooms are available

            // Active allocations (current date between start and end date)
            // Using the Period property instead of direct StartDate/EndDate
            ActiveAllocations = await _context.Allocations
                .Where(a => a.Period.StartDate <= DateTime.Now && a.Period.EndDate >= DateTime.Now)
                .CountAsync();

            // Pending maintenance
            PendingMaintenance = await _context.MaintenancePeriods
                .Where(m => m.Status == 0) // Assuming 0 = Pending
                .CountAsync();
        }
    }
}