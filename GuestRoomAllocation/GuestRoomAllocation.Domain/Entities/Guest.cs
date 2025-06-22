// GuestRoomAllocation.Domain/Entities/Guest.cs
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Common.GuestRoomAllocation.Domain.Common.GuestRoomAllocation.Domain.Common.GuestRoomAllocation.Domain.Common.GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.ValueObjects;

namespace GuestRoomAllocation.Domain.Entities;

public class Guest : HasDomainEventsEntity
{
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public ContactInfo ContactInfo { get; private set; }
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

// GuestRoomAllocation.Domain/Entities/Apartment.cs
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.ValueObjects;

namespace GuestRoomAllocation.Domain.Entities;

public class Apartment : HasDomainEventsEntity
{
    public string Name { get; private set; }
    public Address Address { get; private set; }
    public string? MapLocation { get; private set; }
    public int TotalBathrooms { get; private set; }
    public string? CommonAreas { get; private set; }
    public string? Facilities { get; private set; }
    public string? Amenities { get; private set; }
    public bool HasLaundry { get; private set; }
    public int OverallSpace { get; private set; }

    private readonly List<Room> _rooms = new();
    public IReadOnlyCollection<Room> Rooms => _rooms.AsReadOnly();

    private readonly List<MaintenancePeriod> _maintenancePeriods = new();
    public IReadOnlyCollection<MaintenancePeriod> MaintenancePeriods => _maintenancePeriods.AsReadOnly();

    private Apartment() { } // EF Core

    public Apartment(string name, Address address, int totalBathrooms, int overallSpace)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        TotalBathrooms = totalBathrooms;
        OverallSpace = overallSpace;
    }

    public void UpdateDetails(string name, Address address, int totalBathrooms, int overallSpace,
        string? mapLocation = null, string? commonAreas = null, string? facilities = null,
        string? amenities = null, bool hasLaundry = false)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Address = address ?? throw new ArgumentNullException(nameof(address));
        TotalBathrooms = totalBathrooms;
        OverallSpace = overallSpace;
        MapLocation = mapLocation;
        CommonAreas = commonAreas;
        Facilities = facilities;
        Amenities = amenities;
        HasLaundry = hasLaundry;
        ModifiedDate = DateTime.UtcNow;
    }

    public Room AddRoom(string roomNumber, int size, bool hasPrivateBathroom, int maxOccupancy = 1, string? description = null)
    {
        if (_rooms.Any(r => r.RoomNumber == roomNumber))
            throw new InvalidOperationException($"Room {roomNumber} already exists in apartment {Name}");

        var room = new Room(Id, roomNumber, size, hasPrivateBathroom, maxOccupancy, description);
        _rooms.Add(room);
        ModifiedDate = DateTime.UtcNow;
        return room;
    }

    public void RemoveRoom(Room room)
    {
        _rooms.Remove(room);
        ModifiedDate = DateTime.UtcNow;
    }

    public int AvailableRooms(DateRange dateRange)
    {
        return _rooms.Count(room => room.IsAvailable(dateRange));
    }
}

// GuestRoomAllocation.Domain/Entities/Room.cs
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.ValueObjects;

namespace GuestRoomAllocation.Domain.Entities;

public class Room : HasDomainEventsEntity
{
    public int ApartmentId { get; private set; }
    public string RoomNumber { get; private set; }
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