using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Data;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Pages.Guests
{
    [Authorize]
    public class GuestsIndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public GuestsIndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Guest> Guests { get; set; } = new();
        public Dictionary<int, Allocation?> CurrentAllocations { get; set; } = new();
        public Dictionary<int, Allocation?> LastStays { get; set; } = new();

        public async Task OnGetAsync()
        {
            Guests = await _context.Guests
                .OrderBy(g => g.LastName)
                .ThenBy(g => g.FirstName)
                .ToListAsync();

            var today = DateTime.Today;

            // Load current allocations
            var currentAllocations = await _context.Allocations
                .Include(a => a.Room)
                    .ThenInclude(r => r.Apartment)
                .Where(a => a.CheckInDate <= today && a.CheckOutDate > today)
                .ToListAsync();

            CurrentAllocations = currentAllocations.ToDictionary(a => a.GuestId, a => a);

            // Load last stays for each guest
            foreach (var guest in Guests)
            {
                var lastStay = await _context.Allocations
                    .Include(a => a.Room)
                        .ThenInclude(r => r.Apartment)
                    .Where(a => a.GuestId == guest.Id && a.CheckOutDate <= today)
                    .OrderByDescending(a => a.CheckOutDate)
                    .FirstOrDefaultAsync();

                LastStays[guest.Id] = lastStay;
            }
        }

        public string GetGuestStatus(Guest guest)
        {
            var today = DateTime.Today;
            var allocations = _context.Allocations
                .Where(a => a.GuestId == guest.Id)
                .ToList();

            if (allocations.Any(a => a.CheckInDate <= today && a.CheckOutDate > today))
                return "current";

            if (allocations.Any(a => a.CheckInDate > today))
                return "upcoming";

            if (allocations.Any(a => a.CheckOutDate <= today))
                return "past";

            return "none";
        }

        public Allocation? GetCurrentAllocation(Guest guest)
        {
            CurrentAllocations.TryGetValue(guest.Id, out var allocation);
            return allocation;
        }

        public Allocation? GetLastStay(Guest guest)
        {
            LastStays.TryGetValue(guest.Id, out var lastStay);
            return lastStay;
        }
    }
}
