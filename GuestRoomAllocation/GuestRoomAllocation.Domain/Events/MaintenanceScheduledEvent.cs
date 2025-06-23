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