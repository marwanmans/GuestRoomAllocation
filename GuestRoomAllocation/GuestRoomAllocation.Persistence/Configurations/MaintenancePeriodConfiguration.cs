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

        builder.Ignore(e => e.DomainEvents);
    }
}
