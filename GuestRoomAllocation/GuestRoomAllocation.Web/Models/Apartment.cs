
using System.ComponentModel.DataAnnotations;

namespace GuestRoomAllocation.Web.Models
{
    public class Apartment
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Address { get; set; } = string.Empty;

        [StringLength(500)]
        public string MapLocation { get; set; } = string.Empty;

        public int TotalBathrooms { get; set; }

        [StringLength(500)]
        public string CommonAreas { get; set; } = string.Empty;

        [StringLength(500)]
        public string Facilities { get; set; } = string.Empty;

        [StringLength(500)]
        public string Amenities { get; set; } = string.Empty;

        public bool HasLaundry { get; set; }

        public int OverallSpace { get; set; } // in square meters

        public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
        public virtual ICollection<MaintenancePeriod> MaintenancePeriods { get; set; } = new List<MaintenancePeriod>();
    }
}