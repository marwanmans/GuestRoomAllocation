using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Rooms
{
    [Authorize]
    public class RoomsIndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RoomsIndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Apartment> Apartments { get; set; } = new();
        public Dictionary<int, Allocation?> CurrentAllocations { get; set; } = new();
        public Dictionary<int, Allocation?> UpcomingAllocations { get; set; } = new();
        public Dictionary<int, MaintenancePeriod?> CurrentMaintenance { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadDataAsync();
        }

        public async Task<IActionResult> OnPostAddMaintenanceAsync(int roomId, DateTime startDate, DateTime endDate,
            MaintenanceCategory category, string description, string notes = "")
        {
            if (startDate >= endDate)
            {
                TempData["ErrorMessage"] = "End date must be after start date.";
                await LoadDataAsync();
                return Page();
            }

            var room = await _context.Rooms.FindAsync(roomId);
            if (room == null)
            {
                TempData["ErrorMessage"] = "Room not found.";
                await LoadDataAsync();
                return Page();
            }

            // Check for conflicting allocations
            var conflictingAllocations = await _context.Allocations
                .Where(a => a.RoomId == roomId &&
                           a.CheckInDate < endDate &&
                           a.CheckOutDate > startDate)
                .CountAsync();

            if (conflictingAllocations > 0)
            {
                TempData["ErrorMessage"] = "Cannot schedule maintenance during existing allocations.";
                await LoadDataAsync();
                return Page();
            }

            var maintenance = new MaintenancePeriod
            {
                RoomId = roomId,
                StartDate = startDate,
                EndDate = endDate,
                Category = category,
                Description = description,
                Notes = notes
            };

            _context.MaintenancePeriods.Add(maintenance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Maintenance period scheduled successfully.";
            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            var today = DateTime.Today;

            // Load apartments with rooms
            Apartments = await _context.Apartments
                .Include(a => a.Rooms)
                .OrderBy(a => a.Name)
                .ToListAsync();

            // Load current allocations
            var currentAllocations = await _context.Allocations
                .Include(a => a.Guest)
                .Where(a => a.CheckInDate <= today && a.CheckOutDate > today)
                .ToListAsync();
            CurrentAllocations = currentAllocations.ToDictionary(a => a.RoomId, a => a);

            // Load upcoming allocations
            var upcomingAllocations = await _context.Allocations
                .Include(a => a.Guest)
                .Where(a => a.CheckInDate > today)
                .GroupBy(a => a.RoomId)
                .Select(g => g.OrderBy(a => a.CheckInDate).First())
                .ToListAsync();
            UpcomingAllocations = upcomingAllocations.ToDictionary(a => a.RoomId, a => a);

            // Load current maintenance
            var currentMaintenance = await _context.MaintenancePeriods
                .Where(m => m.RoomId.HasValue && m.StartDate <= today && m.EndDate >= today)
                .ToListAsync();
            CurrentMaintenance = currentMaintenance.ToDictionary(m => m.RoomId!.Value, m => m);
        }

        public Allocation? GetCurrentAllocation(int roomId)
        {
            CurrentAllocations.TryGetValue(roomId, out var allocation);
            return allocation;
        }

        public Allocation? GetUpcomingAllocation(int roomId)
        {
            UpcomingAllocations.TryGetValue(roomId, out var allocation);
            return allocation;
        }

        public bool IsRoomInMaintenance(int roomId)
        {
            return CurrentMaintenance.ContainsKey(roomId);
        }

        public MaintenancePeriod? GetCurrentMaintenance(int roomId)
        {
            CurrentMaintenance.TryGetValue(roomId, out var maintenance);
            return maintenance;
        }
    }
}