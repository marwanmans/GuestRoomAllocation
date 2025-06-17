// Models/User.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuestRoomAllocation.Web.Models
{
    public enum UserRole
    {
        Admin = 1,
        PropertyManager = 2
    }

    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public UserRole Role { get; set; } = UserRole.PropertyManager;

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginDate { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        // Navigation properties
        public virtual ICollection<UserApartmentAccess> ApartmentAccess { get; set; } = new List<UserApartmentAccess>();
        public virtual ICollection<UserGuestAccess> GuestAccess { get; set; } = new List<UserGuestAccess>();
    }
}

// Models/UserApartmentAccess.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace GuestRoomAllocation.Web.Models
{
    public class UserApartmentAccess
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int ApartmentId { get; set; }

        public DateTime GrantedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(ApartmentId))]
        public virtual Apartment Apartment { get; set; } = null!;
    }
}

// Models/UserGuestAccess.cs
using System.ComponentModel.DataAnnotations.Schema;

namespace GuestRoomAllocation.Web.Models
{
    public class UserGuestAccess
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public int GuestId { get; set; }

        public DateTime GrantedDate { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(GuestId))]
        public virtual Guest Guest { get; set; } = null!;
    }
}