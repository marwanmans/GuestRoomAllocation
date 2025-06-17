// Controllers/Api/RoomAvailabilityController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Controllers.Api
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class RoomAvailabilityController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public RoomAvailabilityController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("check")]
        public async Task<IActionResult> CheckAvailability(
            DateTime checkInDate,
            DateTime checkOutDate,
            int excludeAllocationId = 0)
        {
            try
            {
                if (checkInDate >= checkOutDate)
                {
                    return BadRequest(new { error = "Check-out date must be after check-in date" });
                }

                // Get all apartments with their rooms
                var apartments = await _context.Apartments
                    .Include(a => a.Rooms)
                    .OrderBy(a => a.Name)
                    .ToListAsync();

                // Get conflicting allocations
                var conflictingAllocations = await _context.Allocations
                    .Where(a => a.Id != excludeAllocationId &&
                               a.CheckInDate < checkOutDate &&
                               a.CheckOutDate > checkInDate)
                    .Select(a => a.RoomId)
                    .ToListAsync();

                // Get rooms in maintenance
                var maintenanceRooms = await _context.MaintenancePeriods
                    .Where(m => m.RoomId.HasValue &&
                               m.StartDate < checkOutDate &&
                               m.EndDate >= checkInDate)
                    .Select(m => m.RoomId!.Value)
                    .ToListAsync();

                // Build response with available rooms
                var availableRooms = apartments.Select(apartment => new
                {
                    apartmentId = apartment.Id,
                    apartmentName = apartment.Name,
                    rooms = apartment.Rooms
                        .Where(room => !conflictingAllocations.Contains(room.Id) &&
                                      !maintenanceRooms.Contains(room.Id))
                        .Select(room => new
                        {
                            id = room.Id,
                            roomNumber = room.RoomNumber,
                            size = room.Size,
                            hasPrivateBathroom = room.HasPrivateBathroom,
                            description = room.Description,
                            maxOccupancy = room.MaxOccupancy
                        })
                        .OrderBy(r => r.roomNumber)
                        .ToList()
                })
                .Where(a => a.rooms.Any()) // Only include apartments with available rooms
                .ToList();

                // Calculate duration
                var duration = (checkOutDate - checkInDate).Days;

                return Ok(new
                {
                    availableRooms = availableRooms,
                    duration = duration,
                    totalAvailable = availableRooms.Sum(a => a.rooms.Count),
                    checkInDate = checkInDate.ToString("yyyy-MM-dd"),
                    checkOutDate = checkOutDate.ToString("yyyy-MM-dd")
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to check room availability", details = ex.Message });
            }
        }

        [HttpGet("room-details/{roomId}")]
        public async Task<IActionResult> GetRoomDetails(int roomId, DateTime? checkInDate = null, DateTime? checkOutDate = null)
        {
            try
            {
                var room = await _context.Rooms
                    .Include(r => r.Apartment)
                    .FirstOrDefaultAsync(r => r.Id == roomId);

                if (room == null)
                {
                    return NotFound(new { error = "Room not found" });
                }

                var roomDetails = new
                {
                    id = room.Id,
                    roomNumber = room.RoomNumber,
                    size = room.Size,
                    hasPrivateBathroom = room.HasPrivateBathroom,
                    description = room.Description,
                    maxOccupancy = room.MaxOccupancy,
                    apartment = new
                    {
                        id = room.Apartment.Id,
                        name = room.Apartment.Name,
                        address = room.Apartment.Address,
                        totalBathrooms = room.Apartment.TotalBathrooms
                    }
                };

                // If dates provided, check specific availability info
                if (checkInDate.HasValue && checkOutDate.HasValue)
                {
                    var conflictingAllocations = await _context.Allocations
                        .Include(a => a.Guest)
                        .Where(a => a.RoomId == roomId &&
                                   a.CheckInDate < checkOutDate &&
                                   a.CheckOutDate > checkInDate)
                        .Select(a => new
                        {
                            guestName = a.Guest.FullName,
                            checkIn = a.CheckInDate,
                            checkOut = a.CheckOutDate
                        })
                        .ToListAsync();

                    var maintenancePeriods = await _context.MaintenancePeriods
                        .Where(m => m.RoomId == roomId &&
                                   m.StartDate < checkOutDate &&
                                   m.EndDate >= checkInDate)
                        .Select(m => new
                        {
                            description = m.Description,
                            category = m.Category.ToString(),
                            startDate = m.StartDate,
                            endDate = m.EndDate
                        })
                        .ToListAsync();

                    return Ok(new
                    {
                        room = roomDetails,
                        conflicts = new
                        {
                            allocations = conflictingAllocations,
                            maintenance = maintenancePeriods,
                            isAvailable = !conflictingAllocations.Any() && !maintenancePeriods.Any()
                        }
                    });
                }

                return Ok(new { room = roomDetails });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to get room details", details = ex.Message });
            }
        }
    }
}