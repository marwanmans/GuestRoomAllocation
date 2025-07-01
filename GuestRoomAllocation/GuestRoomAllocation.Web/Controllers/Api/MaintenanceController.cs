using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;

namespace GuestRoomAllocation.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class MaintenanceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> GetMaintenanceCalendarEvents()
        {
            try
            {
                var maintenancePeriods = await _context.MaintenancePeriods
                    .Include(m => m.Apartment)
                    .Include(m => m.Room)
                        .ThenInclude(r => r!.Apartment)
                    .Where(m => m.DateRange.StartDate >= DateTime.Now.AddMonths(-1) &&
                               m.DateRange.StartDate <= DateTime.Now.AddMonths(3))
                    .Select(m => new
                    {
                        id = m.Id,
                        title = m.ApartmentId.HasValue
                            ? $"{m.Category} - Apt: {m.Apartment!.Name}"
                            : $"{m.Category} - Room {m.Room!.RoomNumber}",
                        start = m.DateRange.StartDate.ToString("yyyy-MM-dd"),
                        end = m.DateRange.EndDate.ToString("yyyy-MM-dd"),
                        backgroundColor = GetMaintenanceColor(m.Status),
                        borderColor = GetMaintenanceColor(m.Status),
                        className = "maintenance-event",
                        extendedProps = new
                        {
                            category = m.Category.ToString(),
                            status = m.Status.ToString(),
                            description = m.Description,
                            notes = m.Notes,
                            target = m.ApartmentId.HasValue
                                ? $"Apartment: {m.Apartment!.Name}"
                                : $"Room: {m.Room!.Apartment!.Name} - {m.Room.RoomNumber}",
                            apartmentName = m.ApartmentId.HasValue ? m.Apartment!.Name : m.Room!.Apartment!.Name,
                            roomNumber = m.RoomId.HasValue ? m.Room!.RoomNumber : null
                        }
                    })
                    .ToListAsync();

                return Ok(maintenancePeriods);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load maintenance events", details = ex.Message });
            }
        }

        private static string GetMaintenanceColor(Domain.Enums.MaintenanceStatus status)
        {
            return status switch
            {
                Domain.Enums.MaintenanceStatus.Scheduled => "#6f42c1",   // Purple
                Domain.Enums.MaintenanceStatus.InProgress => "#fd7e14",  // Orange
                Domain.Enums.MaintenanceStatus.Completed => "#198754",   // Green
                Domain.Enums.MaintenanceStatus.Cancelled => "#dc3545",   // Red
                _ => "#6c757d" // Default gray
            };
        }
    }
}