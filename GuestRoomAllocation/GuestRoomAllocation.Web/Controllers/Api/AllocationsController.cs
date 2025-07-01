using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;

namespace GuestRoomAllocation.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class AllocationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AllocationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("calendar")]
        public async Task<IActionResult> GetCalendarEvents()
        {
            try
            {
                var allocations = await _context.Allocations
                    .Include(a => a.Guest)
                    .Include(a => a.Room)
                        .ThenInclude(r => r.Apartment)
                    .Where(a => a.DateRange.StartDate >= DateTime.Now.AddMonths(-1) &&
                               a.DateRange.StartDate <= DateTime.Now.AddMonths(2))
                    .Select(a => new
                    {
                        id = a.Id,
                        title = $"{a.Guest.FirstName} {a.Guest.LastName} - Room {a.Room.RoomNumber}",
                        start = a.DateRange.StartDate.ToString("yyyy-MM-dd"),
                        end = a.DateRange.EndDate.ToString("yyyy-MM-dd"),
                        backgroundColor = GetStatusColor(a.Status),
                        borderColor = GetStatusColor(a.Status),
                        extendedProps = new
                        {
                            guestName = $"{a.Guest.FirstName} {a.Guest.LastName}",
                            roomNumber = a.Room.RoomNumber,
                            apartmentName = a.Room.Apartment.Name,
                            status = a.Status.ToString(),
                            notes = a.Notes
                        }
                    })
                    .ToListAsync();

                return Ok(allocations);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load calendar events", details = ex.Message });
            }
        }

        private static string GetStatusColor(Domain.Enums.AllocationStatus status)
        {
            return status switch
            {
                Domain.Enums.AllocationStatus.Upcoming => "#fd7e14",   // Orange
                Domain.Enums.AllocationStatus.Current => "#198754",    // Green
                Domain.Enums.AllocationStatus.Completed => "#6c757d",  // Gray
                Domain.Enums.AllocationStatus.Cancelled => "#dc3545",  // Red
                _ => "#6c757d" // Default gray
            };
        }
    }
}