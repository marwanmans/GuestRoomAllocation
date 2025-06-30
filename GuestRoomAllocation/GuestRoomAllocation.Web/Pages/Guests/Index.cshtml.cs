using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;
using GuestRoomAllocation.Domain.Entities;

namespace GuestRoomAllocation.Web.Pages.Guests
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Guest> Guests { get; set; } = default!;

        public async Task OnGetAsync()
        {
            try
            {
                Guests = await _context.Guests
                    .OrderBy(g => g.LastName)
                    .ThenBy(g => g.FirstName)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Guests = new List<Guest>();
                Console.WriteLine($"Error loading guests: {ex.Message}");
            }
        }
    }
}