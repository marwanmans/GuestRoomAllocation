using GuestRoomAllocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GuestRoomAllocation.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Guest> Guests { get; }
    DbSet<Apartment> Apartments { get; }
    DbSet<Room> Rooms { get; }
    DbSet<Allocation> Allocations { get; }
    DbSet<MaintenancePeriod> MaintenancePeriods { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}