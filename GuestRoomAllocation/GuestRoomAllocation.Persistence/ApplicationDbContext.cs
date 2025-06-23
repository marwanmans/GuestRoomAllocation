using GuestRoomAllocation.Application.Common.Interfaces;
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace GuestRoomAllocation.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        IDateTime dateTime) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public DbSet<Guest> Guests => Set<Guest>();
    public DbSet<Apartment> Apartments => Set<Apartment>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Allocation> Allocations => Set<Allocation>();
    public DbSet<MaintenancePeriod> MaintenancePeriods => Set<MaintenancePeriod>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUserService.Username;
                    entry.Entity.CreatedDate = _dateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    entry.Entity.ModifiedBy = _currentUserService.Username;
                    entry.Entity.ModifiedDate = _dateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}