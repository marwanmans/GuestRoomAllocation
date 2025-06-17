using System.ComponentModel.DataAnnotations;

namespace GuestRoomAllocation.Web.Models
{
    public enum MaintenanceCategory
    {
        Cleaning,
        Repairs,
        Inspection,
        Renovation,
        PestControl,
        Painting,
        PlumbingWork,
        ElectricalWork,
        Other
    }

    public class MaintenancePeriod
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        public MaintenanceCategory Category { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        public int? ApartmentId { get; set; }

        public int? RoomId { get; set; }

        [StringLength(500)]
        public string Notes { get; set; } = string.Empty;

        public virtual Apartment? Apartment { get; set; }
        public virtual Room? Room { get; set; }

        public int Duration => (EndDate - StartDate).Days + 1;

        public string Target => ApartmentId.HasValue ?
            $"Apartment: {Apartment?.Name}" :
            $"Room: {Room?.Apartment?.Name} - {Room?.RoomNumber}";
    }
}