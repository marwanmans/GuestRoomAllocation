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