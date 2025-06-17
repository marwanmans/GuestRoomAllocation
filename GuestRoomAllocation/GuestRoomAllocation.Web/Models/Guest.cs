using System.ComponentModel.DataAnnotations;

namespace GuestRoomAllocation.Web.Models
{
    public class Guest
    {
        public int Id { get; set; }

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
        [Phone]
        [StringLength(20)]
        public string Phone { get; set; } = string.Empty;

        [StringLength(100)]
        public string JobPosition { get; set; } = string.Empty;

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
    }
}
