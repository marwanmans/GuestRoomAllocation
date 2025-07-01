using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;
using GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.Enums;
using GuestRoomAllocation.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace GuestRoomAllocation.Web.Pages.Maintenance
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public SelectList ApartmentOptions { get; set; } = new SelectList(new List<object>(), "Id", "Name");
        public SelectList CategoryOptions { get; set; } = new SelectList(new List<object>(), "Value", "Text");

        [BindProperty]
        public CreateMaintenanceCommand Command { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? date = null)
        {
            await LoadOptions();

            // If a date parameter is provided, set it as the start date
            if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var selectedDate))
            {
                Command.StartDate = selectedDate;
                Command.EndDate = selectedDate.AddDays(1);
            }
            else
            {
                Command.StartDate = DateTime.Today.AddDays(1);
                Command.EndDate = DateTime.Today.AddDays(2);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadOptions();
                return Page();
            }

            try
            {
                var dateRange = new DateRange(Command.StartDate, Command.EndDate);

                MaintenancePeriod maintenance;

                if (Command.TargetType == "Apartment" && Command.ApartmentId.HasValue)
                {
                    // Verify apartment exists
                    var apartmentExists = await _context.Apartments.AnyAsync(a => a.Id == Command.ApartmentId.Value);
                    if (!apartmentExists)
                    {
                        ModelState.AddModelError("Command.ApartmentId", "Selected apartment does not exist.");
                        await LoadOptions();
                        return Page();
                    }

                    maintenance = MaintenancePeriod.ForApartment(
                        Command.ApartmentId.Value,
                        dateRange,
                        Command.Category,
                        Command.Description,
                        Command.Notes);
                }
                else if (Command.TargetType == "Room" && Command.RoomId.HasValue)
                {
                    // Verify room exists
                    var roomExists = await _context.Rooms.AnyAsync(r => r.Id == Command.RoomId.Value);
                    if (!roomExists)
                    {
                        ModelState.AddModelError("Command.RoomId", "Selected room does not exist.");
                        await LoadOptions();
                        return Page();
                    }

                    maintenance = MaintenancePeriod.ForRoom(
                        Command.RoomId.Value,
                        dateRange,
                        Command.Category,
                        Command.Description,
                        Command.Notes);
                }
                else
                {
                    ModelState.AddModelError("", "Please select either an apartment or a room for maintenance.");
                    await LoadOptions();
                    return Page();
                }

                _context.MaintenancePeriods.Add(maintenance);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Maintenance has been scheduled successfully for {Command.StartDate:MMM dd} - {Command.EndDate:MMM dd}!";
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while scheduling maintenance: {ex.Message}");
                await LoadOptions();
                return Page();
            }
        }

        private async Task LoadOptions()
        {
            // Load apartments
            var apartments = await _context.Apartments
                .OrderBy(a => a.Name)
                .Select(a => new { a.Id, a.Name })
                .ToListAsync();

            ApartmentOptions = new SelectList(apartments, "Id", "Name");

            // Load maintenance categories
            var categories = Enum.GetValues(typeof(MaintenanceCategory))
                .Cast<MaintenanceCategory>()
                .Select(c => new { Value = (int)c, Text = c.ToString() })
                .ToList();

            CategoryOptions = new SelectList(categories, "Value", "Text");
        }
    }

    public class CreateMaintenanceCommand
    {
        [Required]
        [Display(Name = "Target Type")]
        public string TargetType { get; set; } = "Apartment";

        [Display(Name = "Apartment")]
        public int? ApartmentId { get; set; }

        [Display(Name = "Room")]
        public int? RoomId { get; set; }

        [Required]
        [Display(Name = "Category")]
        public MaintenanceCategory Category { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Description")]
        public string Description { get; set; } = string.Empty;

        [StringLength(1000)]
        [Display(Name = "Notes")]
        public string? Notes { get; set; }

        [Required]
        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [Display(Name = "End Date")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Display(Name = "Priority")]
        public string Priority { get; set; } = "Medium";

        // Custom validation
        public bool IsValid()
        {
            if (TargetType == "Apartment" && !ApartmentId.HasValue)
                return false;

            if (TargetType == "Room" && !RoomId.HasValue)
                return false;

            if (EndDate < StartDate)
                return false;

            return true;
        }
    }
}