using GuestRoomAllocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestRoomAllocation.Persistence.Configurations;

public class MaintenancePeriodConfiguration : IEntityTypeConfiguration<MaintenancePeriod>
{
    public void Configure(EntityTypeBuilder<MaintenancePeriod> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

        builder.Property(e => e.Category)
            .HasConversion<int>();

        builder.Property(e => e.Status)
            .HasConversion<int>();

        // Configure DateRange value object
        builder.OwnsOne(e => e.DateRange, dateRange =>
        {
            dateRange.Property(d => d.StartDate)
                .IsRequired()
                .HasColumnName("StartDate");

            dateRange.Property(d => d.EndDate)
                .IsRequired()
                .HasColumnName("EndDate");
        });

        // Fix the foreign key relationships to prevent cascade conflicts
        builder.HasOne(e => e.Apartment)
            .WithMany(a => a.MaintenancePeriods)
            .HasForeignKey(e => e.ApartmentId)
            .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to SetNull

        builder.HasOne(e => e.Room)
            .WithMany(r => r.MaintenancePeriods)
            .HasForeignKey(e => e.RoomId)
            .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to SetNull

        builder.Ignore(e => e.DomainEvents);
    }
}