using GuestRoomAllocation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GuestRoomAllocation.Persistence.Configurations;

public class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.JobPosition)
            .HasMaxLength(100);

        builder.Property(e => e.Notes)
            .HasMaxLength(500);

        // Configure ContactInfo value object
        builder.OwnsOne(e => e.ContactInfo, contact =>
        {
            contact.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(150);

            contact.Property(c => c.Phone)
                .IsRequired()
                .HasMaxLength(20);

            contact.HasIndex(c => c.Email)
                .IsUnique();
        });

        builder.HasMany(e => e.Allocations)
            .WithOne(a => a.Guest)
            .HasForeignKey(a => a.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Ignore(e => e.DomainEvents);
    }
}
