using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuestRoomAllocation.Web.Models
{
    public class Allocation
    {
        public int Id { get; set; }

        public int GuestId { get; set; }

        public int RoomId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        public bool BathroomPreferenceOverride { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        [ForeignKey(nameof(GuestId))]
        public virtual Guest? Guest { get; set; } = null!;
        [ForeignKey(nameof(RoomId))]
        public virtual Room? Room { get; set; } = null!;

        public int Duration => (CheckOutDate - CheckInDate).Days;
    }
}