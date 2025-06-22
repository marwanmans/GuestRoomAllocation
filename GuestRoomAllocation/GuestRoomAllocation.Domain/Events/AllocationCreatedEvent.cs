// GuestRoomAllocation.Domain/Events/AllocationCreatedEvent.cs
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Entities;

namespace GuestRoomAllocation.Domain.Events;

public class AllocationCreatedEvent : DomainEvent
{
    public Allocation Allocation { get; }

    public AllocationCreatedEvent(Allocation allocation)
    {
        Allocation = allocation;
    }
}

// GuestRoomAllocation.Domain/Events/AllocationCancelledEvent.cs
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Entities;

namespace GuestRoomAllocation.Domain.Events;

public class AllocationCancelledEvent : DomainEvent
{
    public Allocation Allocation { get; }

    public AllocationCancelledEvent(Allocation allocation)
    {
        Allocation = allocation;
    }
}

// GuestRoomAllocation.Domain/Events/AllocationUpdatedEvent.cs
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Entities;

namespace GuestRoomAllocation.Domain.Events;

public class AllocationUpdatedEvent : DomainEvent
{
    public Allocation Allocation { get; }

    public AllocationUpdatedEvent(Allocation allocation)
    {
        Allocation = allocation;
    }
}

// GuestRoomAllocation.Domain/Events/MaintenanceScheduledEvent.cs
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Entities;

namespace GuestRoomAllocation.Domain.Events;

public class MaintenanceScheduledEvent : DomainEvent
{
    public MaintenancePeriod MaintenancePeriod { get; }

    public MaintenanceScheduledEvent(MaintenancePeriod maintenancePeriod)
    {
        MaintenancePeriod = maintenancePeriod;
    }
}

// GuestRoomAllocation.Domain/Events/MaintenanceCompletedEvent.cs
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Entities;

namespace GuestRoomAllocation.Domain.Events;

public class MaintenanceCompletedEvent : DomainEvent
{
    public MaintenancePeriod MaintenancePeriod { get; }

    public MaintenanceCompletedEvent(MaintenancePeriod maintenancePeriod)
    {
        MaintenancePeriod = maintenancePeriod;
    }
}

// GuestRoomAllocation.Domain/Events/GuestCreatedEvent.cs
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Entities;

namespace GuestRoomAllocation.Domain.Events;

public class GuestCreatedEvent : DomainEvent
{
    public Guest Guest { get; }

    public GuestCreatedEvent(Guest guest)
    {
        Guest = guest;
    }
}

// GuestRoomAllocation.Domain/Events/UserCreatedEvent.cs
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Entities;

namespace GuestRoomAllocation.Domain.Events;

public class UserCreatedEvent : DomainEvent
{
    public User User { get; }

    public UserCreatedEvent(User user)
    {
        User = user;
    }
}