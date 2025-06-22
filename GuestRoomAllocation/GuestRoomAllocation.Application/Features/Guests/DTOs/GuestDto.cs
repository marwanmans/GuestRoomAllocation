// GuestRoomAllocation.Application/Features/Guests/DTOs/GuestDto.cs
using GuestRoomAllocation.Domain.Enums;
using GuestRoomAllocation.Domain.Enums.GuestRoomAllocation.Domain.Enums;
using GuestRoomAllocation.Domain.Enums.GuestRoomAllocation.Domain.Enums.GuestRoomAllocation.Domain.Enums;
using GuestRoomAllocation.Domain.Enums.GuestRoomAllocation.Domain.Enums.GuestRoomAllocation.Domain.Enums.GuestRoomAllocation.Domain.Enums;

namespace GuestRoomAllocation.Application.Features.Guests.DTOs;

public class GuestDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? JobPosition { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class GuestDetailDto : GuestDto
{
    public int TotalAllocations { get; set; }
    public int CurrentAllocations { get; set; }
    public DateTime? LastModified { get; set; }
}

public class GuestSummaryDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? JobPosition { get; set; }
    public AllocationStatus? CurrentStatus { get; set; }
}

// GuestRoomAllocation.Application/Features/Apartments/DTOs/ApartmentDto.cs
namespace GuestRoomAllocation.Application.Features.Apartments.DTOs;

public class ApartmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string? MapLocation { get; set; }
    public int TotalBathrooms { get; set; }
    public int TotalRooms { get; set; }
    public bool HasLaundry { get; set; }
    public int OverallSpace { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class ApartmentDetailDto : ApartmentDto
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string? CommonAreas { get; set; }
    public string? Facilities { get; set; }
    public string? Amenities { get; set; }
    public DateTime? LastModified { get; set; }
}

// GuestRoomAllocation.Application/Features/Rooms/DTOs/RoomDto.cs
namespace GuestRoomAllocation.Application.Features.Rooms.DTOs;

public class RoomDto
{
    public int Id { get; set; }
    public int ApartmentId { get; set; }
    public string ApartmentName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public int Size { get; set; }
    public bool HasPrivateBathroom { get; set; }
    public int MaxOccupancy { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class RoomDetailDto : RoomDto
{
    public int TotalAllocations { get; set; }
    public bool IsCurrentlyOccupied { get; set; }
    public DateTime? LastModified { get; set; }
}

public class RoomAvailabilityDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = string.Empty;
    public string ApartmentName { get; set; } = string.Empty;
    public int Size { get; set; }
    public bool HasPrivateBathroom { get; set; }
    public bool IsAvailable { get; set; }
    public DateTime? NextAvailableDate { get; set; }
}

// GuestRoomAllocation.Application/Features/Allocations/DTOs/AllocationDto.cs
using GuestRoomAllocation.Domain.Enums;

namespace GuestRoomAllocation.Application.Features.Allocations.DTOs;

public class AllocationDto
{
    public int Id { get; set; }
    public int GuestId { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public string ApartmentName { get; set; } = string.Empty;
    public string RoomNumber { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int Duration { get; set; }
    public AllocationStatus Status { get; set; }
    public bool BathroomPreferenceOverride { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class AllocationDetailDto : AllocationDto
{
    public string GuestPhone { get; set; } = string.Empty;
    public string? GuestJobPosition { get; set; }
    public int RoomSize { get; set; }
    public bool HasPrivateBathroom { get; set; }
    public DateTime? LastModified { get; set; }
}

public class AllocationSummaryDto
{
    public int Id { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public AllocationStatus Status { get; set; }
}

// GuestRoomAllocation.Application/Features/Maintenance/DTOs/MaintenanceDto.cs
using GuestRoomAllocation.Domain.Enums;

namespace GuestRoomAllocation.Application.Features.Maintenance.DTOs;

public class MaintenanceDto
{
    public int Id { get; set; }
    public string Location { get; set; } = string.Empty;
    public MaintenanceCategory Category { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int Duration { get; set; }
    public MaintenanceStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class MaintenanceDetailDto : MaintenanceDto
{
    public int? ApartmentId { get; set; }
    public int? RoomId { get; set; }
    public string ApartmentName { get; set; } = string.Empty;
    public string? RoomNumber { get; set; }
    public DateTime? LastModified { get; set; }
}

// GuestRoomAllocation.Application/Features/Users/DTOs/UserDto.cs
using GuestRoomAllocation.Domain.Enums;

namespace GuestRoomAllocation.Application.Features.Users.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class UserDetailDto : UserDto
{
    public int TotalApartmentAccess { get; set; }
    public int TotalGuestAccess { get; set; }
    public string? Notes { get; set; }
    public DateTime? LastModified { get; set; }
}