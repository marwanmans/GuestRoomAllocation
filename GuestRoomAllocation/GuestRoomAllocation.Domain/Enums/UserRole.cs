// GuestRoomAllocation.Domain/Enums/UserRole.cs
namespace GuestRoomAllocation.Domain.Enums;

public enum UserRole
{
    Admin = 1,
    PropertyManager = 2
}

// GuestRoomAllocation.Domain/Enums/MaintenanceCategory.cs
namespace GuestRoomAllocation.Domain.Enums;

public enum MaintenanceCategory
{
    Cleaning = 1,
    Repairs = 2,
    Inspection = 3,
    Renovation = 4,
    PestControl = 5,
    Painting = 6,
    PlumbingWork = 7,
    ElectricalWork = 8,
    Other = 9
}

// GuestRoomAllocation.Domain/Enums/AllocationStatus.cs
namespace GuestRoomAllocation.Domain.Enums;

public enum AllocationStatus
{
    Upcoming = 1,
    Current = 2,
    Completed = 3,
    Cancelled = 4
}

// GuestRoomAllocation.Domain/Enums/MaintenanceStatus.cs
namespace GuestRoomAllocation.Domain.Enums;

public enum MaintenanceStatus
{
    Scheduled = 1,
    InProgress = 2,
    Completed = 3,
    Cancelled = 4
}