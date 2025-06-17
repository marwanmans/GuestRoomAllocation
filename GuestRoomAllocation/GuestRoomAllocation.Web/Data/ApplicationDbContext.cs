// Data/ApplicationDbContext.cs (UPDATED VERSION)
using Microsoft.EntityFrameworkCore;
using GuestRoomAllocation.Web.Models;
using System.Security.Cryptography;
using System.Text;

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
        public DbSet<User> Users { get; set; }
        public DbSet<UserApartmentAccess> UserApartmentAccess { get; set; }
        public DbSet<UserGuestAccess> UserGuestAccess { get; set; }

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

                entity.ToTable(t => t.HasCheckConstraint("CK_Allocation_CheckOutAfterCheckIn",
                    "[CheckOutDate] > [CheckInDate]"));
            });

            // MaintenancePeriod configuration
            modelBuilder.Entity<MaintenancePeriod>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.EndDate).IsRequired();
                entity.Property(e => e.Category).IsRequired();
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasOne(e => e.Apartment)
                    .WithMany(e => e.MaintenancePeriods)
                    .HasForeignKey(e => e.ApartmentId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Room)
                    .WithMany(e => e.MaintenancePeriods)
                    .HasForeignKey(e => e.RoomId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.ToTable(t =>
                {
                    t.HasCheckConstraint("CK_MaintenancePeriod_EndAfterStart",
                        "[EndDate] >= [StartDate]");
                    t.HasCheckConstraint("CK_MaintenancePeriod_ApartmentOrRoom",
                        "([ApartmentId] IS NOT NULL AND [RoomId] IS NULL) OR ([ApartmentId] IS NULL AND [RoomId] IS NOT NULL)");
                });
            });

            // User configuration
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
                entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastName).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).IsRequired();
                entity.Property(e => e.Notes).HasMaxLength(500);

                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
            });

            // UserApartmentAccess configuration
            modelBuilder.Entity<UserApartmentAccess>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.ApartmentAccess)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Apartment)
                    .WithMany()
                    .HasForeignKey(e => e.ApartmentId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.ApartmentId }).IsUnique();
            });

            // UserGuestAccess configuration
            modelBuilder.Entity<UserGuestAccess>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany(e => e.GuestAccess)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(e => e.Guest)
                    .WithMany()
                    .HasForeignKey(e => e.GuestId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(e => new { e.UserId, e.GuestId }).IsUnique();
            });
        }

        // Helper method to hash passwords
        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password + "GuestRoomAllocation2024"));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        // Helper method to verify passwords
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            return HashPassword(password) == hashedPassword;
        }

        // Seed default admin user
        public async Task SeedDefaultAdminAsync()
        {
            if (!await Users.AnyAsync(u => u.Role == UserRole.Admin))
            {
                var adminUser = new User
                {
                    Username = "admin",
                    FirstName = "System",
                    LastName = "Administrator",
                    Email = "admin@guestroom.local",
                    PasswordHash = HashPassword("Admin123!"),
                    Role = UserRole.Admin,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                Users.Add(adminUser);
                await SaveChangesAsync();
            }
        }
    }
}