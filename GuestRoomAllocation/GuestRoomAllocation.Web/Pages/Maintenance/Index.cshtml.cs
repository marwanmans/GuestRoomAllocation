using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;
using GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.Enums;

namespace GuestRoomAllocation.Web.Pages.Maintenance
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int ScheduledCount { get; set; }
        public int InProgressCount { get; set; }
        public int CompletedCount { get; set; }
        public int CancelledCount { get; set; }
        public IList<MaintenancePeriod> UpcomingMaintenance { get; set; } = new List<MaintenancePeriod>();

        public async Task OnGetAsync()
        {
            try
            {
                var today = DateTime.Today;
                var startOfMonth = new DateTime(today.Year, today.Month, 1);
                var endOfWeek = today.AddDays(7);

                // Get counts for different statuses
                ScheduledCount = await _context.MaintenancePeriods
                    .CountAsync(m => m.Status == MaintenanceStatus.Scheduled);

                InProgressCount = await _context.MaintenancePeriods
                    .CountAsync(m => m.Status == MaintenanceStatus.InProgress);

                CompletedCount = await _context.MaintenancePeriods
                    .Where(m => m.Status == MaintenanceStatus.Completed &&
                               m.DateRange.EndDate >= startOfMonth)
                    .CountAsync();

                CancelledCount = await _context.MaintenancePeriods
                    .Where(m => m.Status == MaintenanceStatus.Cancelled &&
                               m.DateRange.StartDate >= startOfMonth)
                    .CountAsync();

                // Get upcoming maintenance for the next week
                UpcomingMaintenance = await _context.MaintenancePeriods
                    .Include(m => m.Apartment)
                    .Include(m => m.Room)
                        .ThenInclude(r => r!.Apartment)
                    .Where(m => m.DateRange.StartDate >= today &&
                               m.DateRange.StartDate <= endOfWeek &&
                               (m.Status == MaintenanceStatus.Scheduled || m.Status == MaintenanceStatus.InProgress))
                    .OrderBy(m => m.DateRange.StartDate)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Fallback values if database query fails
                ScheduledCount = 0;
                InProgressCount = 0;
                CompletedCount = 0;
                CancelledCount = 0;
                UpcomingMaintenance = new List<MaintenancePeriod>();

                // Log the error (optional)
                Console.WriteLine($"Database error: {ex.Message}");
            }
        }
    }
}