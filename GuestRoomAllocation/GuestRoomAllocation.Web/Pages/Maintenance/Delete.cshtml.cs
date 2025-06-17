using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Maintenance
{
    [Authorize]
    public class DeleteMaintenanceModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteMaintenanceModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public MaintenancePeriod Maintenance { get; set; } = new();

        public List<Allocation> ConflictingAllocations { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Maintenance = await _context.MaintenancePeriods
                .Include(m => m.Apartment)
                    .ThenInclude(a => a.Rooms)
                .Include(m => m.Room)
                    .ThenInclude(r => r.Apartment)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Maintenance == null)
            {
                return NotFound();
            }

            await LoadConflictingAllocationsAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            Maintenance = await _context.MaintenancePeriods
                .Include(m => m.Apartment)
                .Include(m => m.Room)
                    .ThenInclude(r => r.Apartment)
                .FirstOrDefaultAsync(m => m.Id == Maintenance.Id);

            if (Maintenance == null)
            {
                return NotFound();
            }

            var description = Maintenance.Description;
            var location = Maintenance.ApartmentId.HasValue ?
                Maintenance.Apartment?.Name :
                $"{Maintenance.Room?.Apartment?.Name} - Room {Maintenance.Room?.RoomNumber}";
            var status = GetMaintenanceStatus();

            _context.MaintenancePeriods.Remove(Maintenance);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Maintenance period '{description}' for {location} has been deleted successfully.";

            if (status == "active")
            {
                TempData["InfoMessage"] = "The location is now available for immediate allocation.";
            }
            else if (status == "scheduled")
            {
                TempData["InfoMessage"] = "The scheduled maintenance has been cancelled.";
            }

            return RedirectToPage("./Index");
        }

        private async Task LoadConflictingAllocationsAsync()
        {
            if (Maintenance.RoomId.HasValue)
            {
                ConflictingAllocations = await _context.Allocations
                    .Include(a => a.Guest)
                    .Include(a => a.Room)
                    .Where(a => a.RoomId == Maintenance.RoomId.Value &&
                               a.CheckInDate < Maintenance.EndDate &&
                               a.CheckOutDate > Maintenance.StartDate)
                    .ToListAsync();
            }
            else if (Maintenance.ApartmentId.HasValue)
            {
                var roomIds = await _context.Rooms
                    .Where(r => r.ApartmentId == Maintenance.ApartmentId.Value)
                    .Select(r => r.Id)
                    .ToListAsync();

                ConflictingAllocations = await _context.Allocations
                    .Include(a => a.Guest)
                    .Include(a => a.Room)
                    .Where(a => roomIds.Contains(a.RoomId) &&
                               a.CheckInDate < Maintenance.EndDate &&
                               a.CheckOutDate > Maintenance.StartDate)
                    .ToListAsync();
            }
        }

        public string GetMaintenanceStatus()
        {
            var today = DateTime.Today;

            if (Maintenance.StartDate <= today && Maintenance.EndDate >= today)
                return "active";
            else if (Maintenance.StartDate > today)
                return "scheduled";
            else
                return "completed";
        }
    }
}