// Pages/Admin/Index.cshtml.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Admin
{
    [Authorize]
    public class AdminIndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public AdminIndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public int TotalGuests { get; set; }
        public int OccupiedRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int MaintenanceCount { get; set; }
        public string ViewMode { get; set; } = "calendar";
        public int CurrentMonth { get; set; } = DateTime.Now.Month;
        public int CurrentYear { get; set; } = DateTime.Now.Year;
        public string CurrentMonthName => new DateTime(CurrentYear, CurrentMonth, 1).ToString("MMMM");

        public List<Apartment> Apartments { get; set; } = new();
        public List<CalendarDay> CalendarDays { get; set; } = new();
        public List<Allocation> CurrentAllocations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string view = "calendar", int month = 0, int year = 0)
        {
            ViewMode = view;

            if (month > 0 && month <= 12) CurrentMonth = month;
            if (year > 0) CurrentYear = year;

            await LoadDataAsync();
            await LoadCalendarDataAsync();

            return Page();
        }

        private async Task LoadDataAsync()
        {
            var today = DateTime.Today;

            // Load basic statistics
            TotalGuests = await _context.Guests.CountAsync();

            var currentAllocations = await _context.Allocations
                .Where(a => a.CheckInDate <= today && a.CheckOutDate > today)
                .CountAsync();
            OccupiedRooms = currentAllocations;

            var totalRooms = await _context.Rooms.CountAsync();

            var roomsInMaintenance = await _context.MaintenancePeriods
                .Where(m => m.RoomId.HasValue && m.StartDate <= today && m.EndDate >= today)
                .Select(m => m.RoomId)
                .Distinct()
                .CountAsync();

            MaintenanceCount = roomsInMaintenance;
            AvailableRooms = totalRooms - OccupiedRooms - MaintenanceCount;

            // Load apartments with rooms
            Apartments = await _context.Apartments
                .Include(a => a.Rooms)
                .OrderBy(a => a.Name)
                .ToListAsync();

            // Load current allocations for list view
            CurrentAllocations = await _context.Allocations
                .Include(a => a.Guest)
                .Include(a => a.Room)
                    .ThenInclude(r => r.Apartment)
                .Where(a => a.CheckInDate <= today && a.CheckOutDate > today)
                .OrderBy(a => a.Room.Apartment.Name)
                .ThenBy(a => a.Room.RoomNumber)
                .ToListAsync();
        }

        private async Task LoadCalendarDataAsync()
        {
            var firstDayOfMonth = new DateTime(CurrentYear, CurrentMonth, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddDays(-1);

            // Calculate calendar grid (start from previous month's Sunday)
            var startDate = firstDayOfMonth.AddDays(-(int)firstDayOfMonth.DayOfWeek);
            var endDate = lastDayOfMonth.AddDays(6 - (int)lastDayOfMonth.DayOfWeek);

            // Load allocations for the calendar period
            var allocations = await _context.Allocations
                .Include(a => a.Guest)
                .Include(a => a.Room)
                    .ThenInclude(r => r.Apartment)
                .Where(a => a.CheckInDate <= endDate && a.CheckOutDate > startDate)
                .ToListAsync();

            // Load maintenance periods for the calendar period
            var maintenancePeriods = await _context.MaintenancePeriods
                .Include(m => m.Apartment)
                .Include(m => m.Room)
                    .ThenInclude(r => r.Apartment)
                .Where(m => m.StartDate <= endDate && m.EndDate >= startDate)
                .ToListAsync();

            // Generate calendar days
            CalendarDays = new List<CalendarDay>();
            for (var date = startDate; date <= endDate; date = date.AddDays(1))
            {
                var dayAllocations = allocations
                    .Where(a => a.CheckInDate <= date && a.CheckOutDate > date)
                    .ToList();

                var dayMaintenance = maintenancePeriods
                    .Where(m => m.StartDate <= date && m.EndDate >= date)
                    .ToList();

                CalendarDays.Add(new CalendarDay
                {
                    Date = date,
                    Day = date.Day,
                    IsCurrentMonth = date.Month == CurrentMonth,
                    Allocations = dayAllocations,
                    MaintenancePeriods = dayMaintenance
                });
            }
        }

        public Allocation? GetCurrentAllocation(int roomId)
        {
            var today = DateTime.Today;
            return CurrentAllocations.FirstOrDefault(a =>
                a.RoomId == roomId &&
                a.CheckInDate <= today &&
                a.CheckOutDate > today);
        }

        public bool IsRoomInMaintenance(int roomId)
        {
            var today = DateTime.Today;
            return _context.MaintenancePeriods
                .Any(m => m.RoomId == roomId &&
                         m.StartDate <= today &&
                         m.EndDate >= today);
        }
    }

    public class CalendarDay
    {
        public DateTime Date { get; set; }
        public int Day { get; set; }
        public bool IsCurrentMonth { get; set; }
        public List<Allocation> Allocations { get; set; } = new();
        public List<MaintenancePeriod> MaintenancePeriods { get; set; } = new();
    }
}