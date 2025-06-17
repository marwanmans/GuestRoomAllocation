using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuestRoomAllocation.Web.Models
{
    public class Room
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string RoomNumber { get; set; } = string.Empty;

        public int ApartmentId { get; set; }

        public int Size { get; set; } // in square meters

        public bool HasPrivateBathroom { get; set; }

        public int MaxOccupancy { get; set; } = 1;

        [StringLength(500)]
        public string Description { get; set; } = string.Empty;

        [ForeignKey(nameof(ApartmentId))]
        public virtual Apartment? Apartment { get; set; } = null!;
        public virtual ICollection<Allocation> Allocations { get; set; } = new List<Allocation>();
        public virtual ICollection<MaintenancePeriod> MaintenancePeriods { get; set; } = new List<MaintenancePeriod>();
    }
}