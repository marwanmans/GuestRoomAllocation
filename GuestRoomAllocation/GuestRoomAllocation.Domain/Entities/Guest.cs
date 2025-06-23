using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.ValueObjects;

namespace GuestRoomAllocation.Domain.Entities;

public class Guest : HasDomainEventsEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public ContactInfo ContactInfo { get; private set; } = null!;
    public string? JobPosition { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<Allocation> _allocations = new();
    public IReadOnlyCollection<Allocation> Allocations => _allocations.AsReadOnly();

    private Guest() { } // EF Core

    public Guest(string firstName, string lastName, ContactInfo contactInfo, string? jobPosition = null, string? notes = null)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        JobPosition = jobPosition;
        Notes = notes;
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateContactInfo(ContactInfo contactInfo)
    {
        ContactInfo = contactInfo ?? throw new ArgumentNullException(nameof(contactInfo));
        ModifiedDate = DateTime.UtcNow;
    }

    public void UpdatePersonalInfo(string firstName, string lastName, string? jobPosition = null, string? notes = null)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        JobPosition = jobPosition;
        Notes = notes;
        ModifiedDate = DateTime.UtcNow;
    }

    internal void AddAllocation(Allocation allocation)
    {
        _allocations.Add(allocation);
    }

    internal void RemoveAllocation(Allocation allocation)
    {
        _allocations.Remove(allocation);
    }
}