using MediatR;

namespace GuestRoomAllocation.Domain.Common;

public abstract class DomainEvent : INotification
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}
