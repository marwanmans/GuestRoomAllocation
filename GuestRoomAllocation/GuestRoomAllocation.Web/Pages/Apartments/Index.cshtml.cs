using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;
using GuestRoomAllocation.Domain.Entities;

namespace GuestRoomAllocation.Web.Pages.Apartments
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Apartment> Apartments { get; set; } = new List<Apartment>();

        public async Task OnGetAsync()
        {
            try
            {
                Apartments = await _context.Apartments
                    .OrderBy(a => a.Name)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                // Log error and show empty list
                Apartments = new List<Apartment>();
                Console.WriteLine($"Error loading apartments: {ex.Message}");
            }
        }
    }
}