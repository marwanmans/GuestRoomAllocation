using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;
using GuestRoomAllocation.Domain.Entities;

namespace GuestRoomAllocation.Web.Pages.Rooms
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Apartment> ApartmentsWithRooms { get; set; } = default!;

        public async Task OnGetAsync()
        {
            ApartmentsWithRooms = await _context.Apartments
                .Include(a => a.Rooms)
                    .ThenInclude(r => r.Allocations)
                        .ThenInclude(a => a.Guest)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }
    }
}