using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Persistence;
using GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.Enums;

namespace GuestRoomAllocation.Web.Pages.Allocations
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<Allocation> CurrentAllocations { get; set; } = new List<Allocation>();
        public IList<Allocation> UpcomingAllocations { get; set; } = new List<Allocation>();
        public IList<Allocation> RecentAllocations { get; set; } = new List<Allocation>();

        [BindProperty(SupportsGet = true)]
        public string? SearchTerm { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? StatusFilter { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ApartmentFilter { get; set; }

        public IList<Apartment> Apartments { get; set; } = new List<Apartment>();

        public async Task OnGetAsync()
        {
            // Load apartments for filter dropdown
            Apartments = await _context.Apartments
                .OrderBy(a => a.Name)
                .ToListAsync();

            // Base query with includes
            var query = _context.Allocations
                .Include(a => a.Guest)
                .Include(a => a.Room)
                    .ThenInclude(r => r.Apartment)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(SearchTerm))
            {
                query = query.Where(a =>
                    a.Guest.FirstName.Contains(SearchTerm) ||
                    a.Guest.LastName.Contains(SearchTerm) ||
                    a.Guest.ContactInfo.Email.Contains(SearchTerm) ||
                    a.Room.RoomNumber.Contains(SearchTerm) ||
                    a.Room.Apartment.Name.Contains(SearchTerm));
            }

            if (ApartmentFilter.HasValue)
            {
                query = query.Where(a => a.Room.ApartmentId == ApartmentFilter.Value);
            }

            if (!string.IsNullOrEmpty(StatusFilter))
            {
                if (Enum.TryParse<AllocationStatus>(StatusFilter, out var status))
                {
                    query = query.Where(a => a.Status == status);
                }
            }

            var allocations = await query
                .OrderBy(a => a.DateRange.StartDate)
                .ToListAsync();

            var today = DateTime.Today;

            // Group allocations by status
            CurrentAllocations = allocations
                .Where(a => a.DateRange.StartDate <= today && a.DateRange.EndDate >= today && a.Status != AllocationStatus.Cancelled)
                .ToList();

            UpcomingAllocations = allocations
                .Where(a => a.DateRange.StartDate > today && a.Status != AllocationStatus.Cancelled)
                .ToList();

            RecentAllocations = allocations
                .Where(a => a.DateRange.EndDate < today || a.Status == AllocationStatus.Cancelled)
                .OrderByDescending(a => a.DateRange.EndDate)
                .Take(10)
                .ToList();
        }
    }
}