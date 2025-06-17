using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Maintenance
{
    [Authorize]
    public class MaintenanceIndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceIndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<MaintenancePeriod> MaintenancePeriods { get; set; } = new();
        public List<Apartment> Apartments { get; set; } = new();
        public string ViewMode { get; set; } = "active";

        // Statistics
        public int ActiveCount { get; set; }
        public int ScheduledCount { get; set; }
        public int CompletedThisMonth { get; set; }
        public int RoomsInMaintenance { get; set; }

        public async Task<IActionResult> OnGetAsync(string view = "active")
        {
            ViewMode = view;
            await LoadDataAsync();
            await LoadStatisticsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostCreateAsync(string locationType, int? apartmentId, int? roomId,
            MaintenanceCategory category, DateTime startDate, DateTime endDate, string description, string notes = "")
        {
            if (startDate >= endDate)
            {
                TempData["ErrorMessage"] = "End date must be after start date.";
                return RedirectToPage();
            }

            if (locationType == "apartment" && !apartmentId.HasValue)
            {
                TempData["ErrorMessage"] = "Please select an apartment.";
                return RedirectToPage();
            }

            if (locationType == "room" && !roomId.HasValue)
            {
                TempData["ErrorMessage"] = "Please select a room.";
                return RedirectToPage();
            }

            // Check for conflicting allocations if it's a room
            if (roomId.HasValue)
            {
                var conflictingAllocations = await _context.Allocations
                    .Where(a => a.RoomId == roomId.Value &&
                               a.CheckInDate < endDate &&
                               a.CheckOutDate > startDate)
                    .CountAsync();

                if (conflictingAllocations > 0)
                {
                    TempData["WarningMessage"] = "Warning: There are existing allocations during this maintenance period. Consider rescheduling or contacting affected guests.";
                }
            }
            else if (apartmentId.HasValue)
            {
                // Check for conflicting allocations in any room of the apartment
                var roomIds = await _context.Rooms
                    .Where(r => r.ApartmentId == apartmentId.Value)
                    .Select(r => r.Id)
                    .ToListAsync();

                var conflictingAllocations = await _context.Allocations
                    .Where(a => roomIds.Contains(a.RoomId) &&
                               a.CheckInDate < endDate &&
                               a.CheckOutDate > startDate)
                    .CountAsync();

                if (conflictingAllocations > 0)
                {
                    TempData["WarningMessage"] = $"Warning: There are {conflictingAllocations} existing allocations in this apartment during this maintenance period.";
                }
            }

            var maintenance = new MaintenancePeriod
            {
                ApartmentId = locationType == "apartment" ? apartmentId : null,
                RoomId = locationType == "room" ? roomId : null,
                Category = category,
                StartDate = startDate,
                EndDate = endDate,
                Description = description,
                Notes = notes
            };

            _context.MaintenancePeriods.Add(maintenance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Maintenance period scheduled successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostCompleteAsync(int id)
        {
            var maintenance = await _context.MaintenancePeriods
                .Include(m => m.Apartment)
                .Include(m => m.Room)
                    .ThenInclude(r => r.Apartment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (maintenance == null)
            {
                TempData["ErrorMessage"] = "Maintenance period not found.";
                return RedirectToPage();
            }

            // Mark as complete by setting end date to today if it's in the future
            if (maintenance.EndDate > DateTime.Today)
            {
                maintenance.EndDate = DateTime.Today;
            }

            var location = maintenance.ApartmentId.HasValue ?
                maintenance.Apartment?.Name :
                $"{maintenance.Room?.Apartment?.Name} - Room {maintenance.Room?.RoomNumber}";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Maintenance '{maintenance.Description}' for {location} has been marked as completed.";
            TempData["InfoMessage"] = "The location is now available for allocation.";

            return RedirectToPage();
        }

        private async Task LoadDataAsync()
        {
            MaintenancePeriods = await _context.MaintenancePeriods
                .Include(m => m.Apartment)
                .Include(m => m.Room)
                    .ThenInclude(r => r.Apartment)
                .OrderByDescending(m => m.StartDate)
                .ToListAsync();

            Apartments = await _context.Apartments
                .Include(a => a.Rooms)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        private async Task LoadStatisticsAsync()
        {
            var today = DateTime.Today;
            var startOfMonth = new DateTime(today.Year, today.Month, 1);

            ActiveCount = await _context.MaintenancePeriods
                .CountAsync(m => m.StartDate <= today && m.EndDate >= today);

            ScheduledCount = await _context.MaintenancePeriods
                .CountAsync(m => m.StartDate > today);

            CompletedThisMonth = await _context.MaintenancePeriods
                .CountAsync(m => m.EndDate >= startOfMonth && m.EndDate < today);

            RoomsInMaintenance = await _context.MaintenancePeriods
                .Where(m => m.RoomId.HasValue && m.StartDate <= today && m.EndDate >= today)
                .Select(m => m.RoomId)
                .Distinct()
                .CountAsync();
        }

        public string GetMaintenanceStatus(MaintenancePeriod maintenance)
        {
            var today = DateTime.Today;

            if (maintenance.StartDate <= today && maintenance.EndDate >= today)
                return "active";
            else if (maintenance.StartDate > today)
                return "scheduled";
            else
                return "completed";
        }

        public string GetMaintenancePriority(MaintenancePeriod maintenance)
        {
            var today = DateTime.Today;
            var daysUntilStart = (maintenance.StartDate - today).Days;
            var daysUntilEnd = (maintenance.EndDate - today).Days;

            if (maintenance.StartDate <= today && maintenance.EndDate >= today)
            {
                if (daysUntilEnd < 0)
                    return "overdue";
                else if (daysUntilEnd == 0)
                    return "urgent";
            }
            else if (maintenance.StartDate > today && daysUntilStart <= 7)
            {
                return "soon";
            }

            return "normal";
        }
    }
}