using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Maintenance
{
    [Authorize]
    public class EditMaintenanceModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditMaintenanceModel(ApplicationDbContext context)
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

        public async Task<IActionResult> OnPostAsync(string action)
        {
            if (!ModelState.IsValid)
            {
                await LoadMaintenanceDataAsync();
                return Page();
            }

            if (Maintenance.StartDate >= Maintenance.EndDate)
            {
                ModelState.AddModelError("Maintenance.EndDate", "End date must be after start date.");
                await LoadMaintenanceDataAsync();
                return Page();
            }

            if (action == "complete")
            {
                // Mark as complete by setting end date to today if it's in the future
                if (Maintenance.EndDate > DateTime.Today)
                {
                    Maintenance.EndDate = DateTime.Today;
                }

                TempData["SuccessMessage"] = $"Maintenance '{Maintenance.Description}' has been marked as completed.";
            }
            else
            {
                // Check for conflicts when updating dates
                await LoadConflictingAllocationsAsync();
                if (ConflictingAllocations.Any())
                {
                    TempData["WarningMessage"] = $"Warning: This maintenance period conflicts with {ConflictingAllocations.Count} existing allocation(s).";
                }

                TempData["SuccessMessage"] = $"Maintenance '{Maintenance.Description}' has been updated successfully.";
            }

            _context.Attach(Maintenance).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await MaintenanceExists(Maintenance.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private async Task LoadMaintenanceDataAsync()
        {
            var maintenanceFromDb = await _context.MaintenancePeriods
                .Include(m => m.Apartment)
                    .ThenInclude(a => a.Rooms)
                .Include(m => m.Room)
                    .ThenInclude(r => r.Apartment)
                .FirstOrDefaultAsync(m => m.Id == Maintenance.Id);

            if (maintenanceFromDb != null)
            {
                Maintenance.Apartment = maintenanceFromDb.Apartment;
                Maintenance.Room = maintenanceFromDb.Room;
                Maintenance.ApartmentId = maintenanceFromDb.ApartmentId;
                Maintenance.RoomId = maintenanceFromDb.RoomId;
            }

            await LoadConflictingAllocationsAsync();
        }

        private async Task LoadConflictingAllocationsAsync()
        {
            if (Maintenance.RoomId.HasValue)
            {
                ConflictingAllocations = await _context.Allocations
                    .Include(a => a.Guest)
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

        private async Task<bool> MaintenanceExists(int id)
        {
            return await _context.MaintenancePeriods.AnyAsync(e => e.Id == id);
        }
    }
}