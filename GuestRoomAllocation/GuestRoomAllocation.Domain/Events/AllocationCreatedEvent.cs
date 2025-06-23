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