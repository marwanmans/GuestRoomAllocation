using GuestRoomAllocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestRoomAllocation.Persistence.Configurations;

public class ApartmentConfiguration : IEntityTypeConfiguration<Apartment>
{
    public void Configure(EntityTypeBuilder<Apartment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.MapLocation)
            .HasMaxLength(500);

        builder.Property(e => e.CommonAreas)
            .HasMaxLength(1000);

        builder.Property(e => e.Facilities)
            .HasMaxLength(1000);

        builder.Property(e => e.Amenities)
            .HasMaxLength(1000);

        // Configure Address value object
        builder.OwnsOne(e => e.Address, address =>
        {
            address.Property(a => a.Street)
                .IsRequired()
                .HasMaxLength(200);

            address.Property(a => a.City)
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.State)
                .IsRequired()
                .HasMaxLength(100);

            address.Property(a => a.ZipCode)
                .IsRequired()
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .IsRequired()
                .HasMaxLength(100);
        });

        builder.HasMany(e => e.Rooms)
            .WithOne(r => r.Apartment)
            .HasForeignKey(r => r.ApartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.MaintenancePeriods)
            .WithOne(m => m.Apartment)
            .HasForeignKey(m => m.ApartmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(e => e.DomainEvents);
    }
}