using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;
using GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.ValueObjects;
using System.ComponentModel.DataAnnotations;

namespace GuestRoomAllocation.Web.Pages.Guests
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CreateGuestCommand Command { get; set; } = default!;

        public IActionResult OnGet()
        {
            Command = new CreateGuestCommand();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                // Check if email already exists
                var existingGuest = await _context.Guests
                    .FirstOrDefaultAsync(g => g.ContactInfo.Email == Command.Email);

                if (existingGuest != null)
                {
                    ModelState.AddModelError("Command.Email", "A guest with this email already exists.");
                    return Page();
                }

                var contactInfo = new ContactInfo(Command.Email, Command.Phone);

                var guest = new Guest(
                    Command.FirstName,
                    Command.LastName,
                    contactInfo,
                    Command.JobPosition,
                    Command.Notes);

                _context.Guests.Add(guest);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Guest created successfully!";
                return RedirectToPage("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"An error occurred while creating the guest: {ex.Message}");
                return Page();
            }
        }
    }

    public class CreateGuestCommand
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        [Display(Name = "Job Position")]
        public string? JobPosition { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}