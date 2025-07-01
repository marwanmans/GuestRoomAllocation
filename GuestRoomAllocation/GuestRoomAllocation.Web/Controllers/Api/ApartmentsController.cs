using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;

namespace GuestRoomAllocation.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("{apartmentId}/rooms")]
        public async Task<IActionResult> GetRoomsByApartment(int apartmentId)
        {
            try
            {
                var rooms = await _context.Rooms
                    .Where(r => r.ApartmentId == apartmentId)
                    .OrderBy(r => r.RoomNumber)
                    .Select(r => new
                    {
                        id = r.Id,
                        roomNumber = r.RoomNumber,
                        size = r.Size,
                        hasPrivateBathroom = r.HasPrivateBathroom
                    })
                    .ToListAsync();

                return Ok(rooms);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to load rooms", details = ex.Message });
            }
        }
    }
}