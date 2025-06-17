
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Allocations
{
    [Authorize]
    public class AllocationsIndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AllocationsIndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Allocation> Allocations { get; set; } = new();
        public List<Apartment> Apartments { get; set; } = new();
        public Dictionary<(int roomId, DateTime date), Allocation?> AllocationCache { get; set; } = new();
        public Dictionary<(int roomId, DateTime date), MaintenancePeriod?> MaintenanceCache { get; set; } = new();
        public string ViewMode { get; set; } = "list";
        public int CurrentMonth { get; set; } = DateTime.Now.Month;
        public int CurrentYear { get; set; } = DateTime.Now.Year;
        public string CurrentMonthName => new DateTime(CurrentYear, CurrentMonth, 1).ToString("MMMM");

        public async Task<IActionResult> OnGetAsync(string view = "list", int month = 0, int year = 0)
        {
            ViewMode = view;

            if (month > 0 && month <= 12) CurrentMonth = month;
            if (year > 0) CurrentYear = year;

            await LoadDataAsync();

            if (ViewMode == "calendar")
            {
                await LoadCalendarDataAsync();
            }

            return Page();
        }

        private async Task LoadDataAsync()
        {
            // Load all allocations with related data
            Allocations = await _context.Allocations
                .Include(a => a.Guest)
                .Include(a => a.Room)
                    .ThenInclude(r => r.Apartment)
                .OrderByDescending(a => a.CheckInDate)
                .ToListAsync();

            // Load apartments with rooms for calendar view
            Apartments = await _context.Apartments
                .Include(a => a.Rooms)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        private async Task LoadCalendarDataAsync()
        {
            var firstDayOfMonth = new DateTime(CurrentYear, CurrentMonth, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Load allocations for the calendar month
            var monthAllocations = await _context.Allocations
                .Include(a => a.Guest)
                .Include(a => a.Room)
                    .ThenInclude(r => r.Apartment)
                .Where(a => a.CheckInDate <= lastDayOfMonth && a.CheckOutDate > firstDayOfMonth)
                .ToListAsync();

            // Load maintenance periods for the calendar month
            var monthMaintenance = await _context.MaintenancePeriods
                .Include(m => m.Room)
                    .ThenInclude(r => r.Apartment)
                .Where(m => m.RoomId.HasValue && m.StartDate <= lastDayOfMonth && m.EndDate >= firstDayOfMonth)
                .ToListAsync();

            // Build caches for quick lookups
            AllocationCache = new Dictionary<(int roomId, DateTime date), Allocation?>();
            MaintenanceCache = new Dictionary<(int roomId, DateTime date), MaintenancePeriod?>();

            for (int day = 1; day <= DateTime.DaysInMonth(CurrentYear, CurrentMonth); day++)
            {
                var currentDate = new DateTime(CurrentYear, CurrentMonth, day);

                foreach (var apartment in Apartments)
                {
                    foreach (var room in apartment.Rooms)
                    {
                        // Cache allocation for this room/date
                        var allocation = monthAllocations.FirstOrDefault(a =>
                            a.RoomId == room.Id &&
                            a.CheckInDate <= currentDate &&
                            a.CheckOutDate > currentDate);
                        AllocationCache[(room.Id, currentDate)] = allocation;

                        // Cache maintenance for this room/date
                        var maintenance = monthMaintenance.FirstOrDefault(m =>
                            m.RoomId == room.Id &&
                            m.StartDate <= currentDate &&
                            m.EndDate >= currentDate);
                        MaintenanceCache[(room.Id, currentDate)] = maintenance;
                    }
                }
            }
        }

        public string GetAllocationStatus(Allocation allocation)
        {
            var today = DateTime.Today;

            if (allocation.CheckInDate <= today && allocation.CheckOutDate > today)
                return "current";
            else if (allocation.CheckInDate > today)
                return "upcoming";
            else
                return "past";
        }

        public Allocation? GetAllocationForDate(int roomId, DateTime date)
        {
            if (AllocationCache.TryGetValue((roomId, date), out var allocation))
            {
                return allocation;
            }
            return null;
        }

        public MaintenancePeriod? GetMaintenanceForDate(int roomId, DateTime date)
        {
            if (MaintenanceCache.TryGetValue((roomId, date), out var maintenance))
            {
                return maintenance;
            }
            return null;
        }
    }
}