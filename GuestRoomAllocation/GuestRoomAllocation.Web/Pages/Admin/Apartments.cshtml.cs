
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Admin
{
    [Authorize]
    public class ApartmentsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ApartmentsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Apartment> Apartments { get; set; } = new();
        public Dictionary<int, bool> RoomOccupancy { get; set; } = new();
        public Dictionary<int, bool> RoomMaintenance { get; set; } = new();

        public async Task OnGetAsync()
        {
            Apartments = await _context.Apartments
                .Include(a => a.Rooms)
                .OrderBy(a => a.Name)
                .ToListAsync();

            await LoadRoomStatusAsync();
        }

        private async Task LoadRoomStatusAsync()
        {
            var today = DateTime.Today;
            var roomIds = Apartments.SelectMany(a => a.Rooms).Select(r => r.Id).ToList();

            // Load current allocations
            var occupiedRooms = await _context.Allocations
                .Where(a => roomIds.Contains(a.RoomId) &&
                           a.CheckInDate <= today &&
                           a.CheckOutDate > today)
                .Select(a => a.RoomId)
                .ToListAsync();

            // Load rooms in maintenance
            var maintenanceRooms = await _context.MaintenancePeriods
                .Where(m => m.RoomId.HasValue &&
                           roomIds.Contains(m.RoomId.Value) &&
                           m.StartDate <= today &&
                           m.EndDate >= today)
                .Select(m => m.RoomId!.Value)
                .ToListAsync();

            RoomOccupancy = roomIds.ToDictionary(id => id, id => occupiedRooms.Contains(id));
            RoomMaintenance = roomIds.ToDictionary(id => id, id => maintenanceRooms.Contains(id));
        }

        public int GetAvailableRooms(int apartmentId)
        {
            var apartment = Apartments.FirstOrDefault(a => a.Id == apartmentId);
            if (apartment == null) return 0;

            return apartment.Rooms.Count(room => !IsRoomOccupied(room.Id) && !IsRoomInMaintenance(room.Id));
        }

        public bool IsRoomOccupied(int roomId)
        {
            return RoomOccupancy.GetValueOrDefault(roomId, false);
        }

        public bool IsRoomInMaintenance(int roomId)
        {
            return RoomMaintenance.GetValueOrDefault(roomId, false);
        }
    }
}