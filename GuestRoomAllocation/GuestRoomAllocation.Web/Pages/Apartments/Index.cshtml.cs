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

        public IList<Apartment> Apartments { get; set; } = default!;

        public async Task OnGetAsync()
        {
            Apartments = await _context.Apartments
                .Include(a => a.Rooms)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
    }
}