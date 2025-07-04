﻿// REPLACE FILE: Configurations/AllocationConfiguration.cs
using GuestRoomAllocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestRoomAllocation.Persistence.Configurations;

public class AllocationConfiguration : IEntityTypeConfiguration<Allocation>
{
    public void Configure(EntityTypeBuilder<Allocation> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Notes)
            .HasMaxLength(1000);

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

        // Create index for room and date overlap checking
        builder.HasIndex(e => e.RoomId);

        builder.Ignore(e => e.DomainEvents);
    }
}