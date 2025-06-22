// GuestRoomAllocation.Domain/Common/BaseEntity.cs
namespace GuestRoomAllocation.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedDate { get; set; }
}

// GuestRoomAllocation.Domain/Common/IAuditableEntity.cs
namespace GuestRoomAllocation.Domain.Common;

public interface IAuditableEntity
{
    DateTime CreatedDate { get; set; }
    string? CreatedBy { get; set; }
    DateTime? ModifiedDate { get; set; }
    string? ModifiedBy { get; set; }
}

// GuestRoomAllocation.Domain/Common/AuditableEntity.cs
namespace GuestRoomAllocation.Domain.Common;

public abstract class AuditableEntity : BaseEntity, IAuditableEntity
{
    public string? CreatedBy { get; set; }
    public string? ModifiedBy { get; set; }
}

// GuestRoomAllocation.Domain/Common/DomainEvent.cs
using MediatR;

namespace GuestRoomAllocation.Domain.Common;

public abstract class DomainEvent : INotification
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// GuestRoomAllocation.Domain/Common/HasDomainEventsEntity.cs
namespace GuestRoomAllocation.Domain.Common;

public abstract class HasDomainEventsEntity : AuditableEntity
{
    private readonly List<DomainEvent> _domainEvents = new();

    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void RemoveDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}