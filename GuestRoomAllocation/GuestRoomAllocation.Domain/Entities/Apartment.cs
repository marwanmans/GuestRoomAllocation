using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.ValueObjects;

namespace GuestRoomAllocation.Domain.Entities;

public class Apartment : HasDomainEventsEntity
{
    public string Name { get; private set; } = string.Empty;
    public Address Address { get; private set; } = null!;
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
