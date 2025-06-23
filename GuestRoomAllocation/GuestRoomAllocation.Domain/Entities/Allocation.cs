using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Enums;
using GuestRoomAllocation.Domain.Events;
using GuestRoomAllocation.Domain.Exceptions;
using GuestRoomAllocation.Domain.ValueObjects;

namespace GuestRoomAllocation.Domain.Entities;

public class Allocation : HasDomainEventsEntity
{
    public int GuestId { get; private set; }
    public int RoomId { get; private set; }
    public DateRange DateRange { get; private set; } = null!;
    public bool BathroomPreferenceOverride { get; private set; }
    public string? Notes { get; private set; }
    public AllocationStatus Status { get; private set; }

    public Guest Guest { get; private set; } = null!;
    public Room Room { get; private set; } = null!;

    private Allocation() { } // EF Core

    public Allocation(int guestId, int roomId, DateRange dateRange, bool bathroomPreferenceOverride = false, string? notes = null)
    {
        GuestId = guestId;
        RoomId = roomId;
        DateRange = dateRange ?? throw new ArgumentNullException(nameof(dateRange));
        BathroomPreferenceOverride = bathroomPreferenceOverride;
        Notes = notes;
        Status = DetermineStatus(dateRange);

        AddDomainEvent(new AllocationCreatedEvent(this));
    }

    public void UpdateDateRange(DateRange newDateRange)
    {
        DateRange = newDateRange ?? throw new ArgumentNullException(nameof(newDateRange));
        Status = DetermineStatus(newDateRange);
        ModifiedDate = DateTime.UtcNow;
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = AllocationStatus.Cancelled;
        ModifiedDate = DateTime.UtcNow;
        AddDomainEvent(new AllocationCancelledEvent(this));
    }

    public void UpdateStatus()
    {
        var newStatus = DetermineStatus(DateRange);
        if (newStatus != Status)
        {
            Status = newStatus;
            ModifiedDate = DateTime.UtcNow;
        }
    }

    public int Duration => DateRange.Duration;

    private static AllocationStatus DetermineStatus(DateRange dateRange)
    {
        var today = DateTime.Today;

        if (dateRange.StartDate > today)
            return AllocationStatus.Upcoming;

        if (dateRange.Contains(today))
            return AllocationStatus.Current;

        return AllocationStatus.Completed;
    }
}