// Data/ApplicationDbContext.cs (FIXED VERSION)
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Models;

namespace GuestRoomAllocation.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Apartment> Apartments { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Guest> Guests { get; set; }
        public DbSet<Allocation> Allocations { get; set; }
        public DbSet<MaintenancePeriod> MaintenancePeriods { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Apartment configuration
            modelBuilder.Entity<Apartment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Address).IsRequired().HasMaxLength(200);
                entity.Property(e => e.MapLocation).HasMaxLength(500);
                entity.Property(e => e.CommonAreas).HasMaxLength(500);
                entity.Property(e => e.Facilities).HasMaxLength(500);
                entity.Property(e => e.Amenities).HasMaxLength(500);
            });

            // Room configuration
            modelBuilder.Entity<Room>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.RoomNumber).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Description).HasMaxLength(500);

                entity.HasOne(e => e.Apartment)
                    .WithMany(e => e.Rooms)
                    .HasForeignKey(e => e.ApartmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.ApartmentId, e.RoomNumber })
                    .IsUnique();
            });

            // Guest configuration
            modelBuilder.Entity<Guest>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Phone).IsRequired().HasMaxLength(20);
                entity.Property(e => e.JobPosition).HasMaxLength(100);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasIndex(e => e.Email).IsUnique();
            });

            // Allocation configuration
            modelBuilder.Entity<Allocation>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.CheckInDate).IsRequired();
                entity.Property(e => e.CheckOutDate).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.CreatedDate).IsRequired();

                entity.HasOne(e => e.Guest)
                    .WithMany(e => e.Allocations)
                    .HasForeignKey(e => e.GuestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Room)
                    .WithMany(e => e.Allocations)
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Modern way to add check constraints (EF Core 5.0+)
                entity.ToTable(t => t.HasCheckConstraint("CK_Allocation_CheckOutAfterCheckIn",
                    "[CheckOutDate] > [CheckInDate]"));
            });

            // MaintenancePeriod configuration - FIX CASCADE ISSUE
            modelBuilder.Entity<MaintenancePeriod>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.Category).IsRequired();
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(500);

                // FIXED: Use NO ACTION to prevent cascade cycles
                entity.HasOne(e => e.Apartment)
                    .WithMany(e => e.MaintenancePeriods)
                    .HasForeignKey(e => e.ApartmentId)
                    .OnDelete(DeleteBehavior.NoAction);  // CHANGED FROM CASCADE

                entity.HasOne(e => e.Room)
                    .WithMany(e => e.MaintenancePeriods)
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.NoAction);  // CHANGED FROM CASCADE

                // Modern way to add check constraints (EF Core 5.0+)
                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_MaintenancePeriod_EndAfterStart",
                        "[EndDate] >= [StartDate]");
                    t.HasCheckConstraint("CK_MaintenancePeriod_ApartmentOrRoom",
                        "([ApartmentId] IS NOT NULL AND [RoomId] IS NULL) OR ([ApartmentId] IS NULL AND [RoomId] IS NOT NULL)");
                });
            });
        }
    }
}