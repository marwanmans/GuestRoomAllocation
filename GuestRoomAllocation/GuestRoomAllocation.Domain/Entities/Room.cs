using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.ValueObjects;

namespace GuestRoomAllocation.Domain.Entities;

public class Room : HasDomainEventsEntity
{
    public int ApartmentId { get; private set; }
    public string RoomNumber { get; private set; } = string.Empty;
    public int Size { get; private set; }
    public bool HasPrivateBathroom { get; private set; }
    public int MaxOccupancy { get; private set; }
    public string? Description { get; private set; }

    public Apartment Apartment { get; private set; } = null!;

    private readonly List<Allocation> _allocations = new();
    public IReadOnlyCollection<Allocation> Allocations => _allocations.AsReadOnly();

    private readonly List<MaintenancePeriod> _maintenancePeriods = new();
    public IReadOnlyCollection<MaintenancePeriod> MaintenancePeriods => _maintenancePeriods.AsReadOnly();

    private Room() { } // EF Core

    internal Room(int apartmentId, string roomNumber, int size, bool hasPrivateBathroom, int maxOccupancy = 1, string? description = null)
    {
        ApartmentId = apartmentId;
        RoomNumber = roomNumber ?? throw new ArgumentNullException(nameof(roomNumber));
        Size = size > 0 ? size : throw new ArgumentException("Size must be greater than 0", nameof(size));
        HasPrivateBathroom = hasPrivateBathroom;
        MaxOccupancy = maxOccupancy > 0 ? maxOccupancy : throw new ArgumentException("MaxOccupancy must be greater than 0", nameof(maxOccupancy));
        Description = description;
    }

    public void UpdateDetails(int size, bool hasPrivateBathroom, int maxOccupancy, string? description = null)
    {
        Size = size > 0 ? size : throw new ArgumentException("Size must be greater than 0", nameof(size));
        HasPrivateBathroom = hasPrivateBathroom;
        MaxOccupancy = maxOccupancy > 0 ? maxOccupancy : throw new ArgumentException("MaxOccupancy must be greater than 0", nameof(maxOccupancy));
        Description = description;
        ModifiedDate = DateTime.UtcNow;
    }

    public bool IsAvailable(DateRange dateRange)
    {
        var hasConflictingAllocations = _allocations.Any(a => a.DateRange.Overlaps(dateRange));
        var hasConflictingMaintenance = _maintenancePeriods.Any(m => m.DateRange.Overlaps(dateRange));

        return !hasConflictingAllocations && !hasConflictingMaintenance;
    }

    internal void AddAllocation(Allocation allocation)
    {
        _allocations.Add(allocation);
    }

    internal void RemoveAllocation(Allocation allocation)
    {
        _allocations.Remove(allocation);
    }

    internal void AddMaintenancePeriod(MaintenancePeriod maintenancePeriod)
    {
        _maintenancePeriods.Add(maintenancePeriod);
    }

    internal void RemoveMaintenancePeriod(MaintenancePeriod maintenancePeriod)
    {
        _maintenancePeriods.Remove(maintenancePeriod);
    }
}