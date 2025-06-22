// GuestRoomAllocation.Domain/Entities/Allocation.cs
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Common.GuestRoomAllocation.Domain.Common.GuestRoomAllocation.Domain.Common.GuestRoomAllocation.Domain.Common.GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities.GuestRoomAllocation.Domain.Entities;
using GuestRoomAllocation.Domain.Enums;
using GuestRoomAllocation.Domain.Enums.GuestRoomAllocation.Domain.Enums.GuestRoomAllocation.Domain.Enums.GuestRoomAllocation.Domain.Enums;
using GuestRoomAllocation.Domain.Events;
using GuestRoomAllocation.Domain.Exceptions;
using GuestRoomAllocation.Domain.ValueObjects;

namespace GuestRoomAllocation.Domain.Entities;

public class Allocation : HasDomainEventsEntity
{
    public int GuestId { get; private set; }
    public int RoomId { get; private set; }
    public DateRange DateRange { get; private set; }
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

    public static Allocation Create(Guest guest, Room room, DateRange dateRange, bool bathroomPreferenceOverride = false, string? notes = null)
    {
        if (!room.IsAvailable(dateRange))
            throw new RoomNotAvailableException(room.Id, dateRange.StartDate, dateRange.EndDate);

        var allocation = new Allocation(guest.Id, room.Id, dateRange, bathroomPreferenceOverride, notes);

        guest.AddAllocation(allocation);
        room.AddAllocation(allocation);

        return allocation;
    }

    public void UpdateDateRange(DateRange newDateRange)
    {
        if (!Room.IsAvailable(newDateRange))
            throw new RoomNotAvailableException(RoomId, newDateRange.StartDate, newDateRange.EndDate);

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

// GuestRoomAllocation.Domain/Entities/MaintenancePeriod.cs
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Enums;
using GuestRoomAllocation.Domain.Events;
using GuestRoomAllocation.Domain.ValueObjects;

namespace GuestRoomAllocation.Domain.Entities;

public class MaintenancePeriod : HasDomainEventsEntity
{
    public int? ApartmentId { get; private set; }
    public int? RoomId { get; private set; }
    public DateRange DateRange { get; private set; }
    public MaintenanceCategory Category { get; private set; }
    public string Description { get; private set; }
    public string? Notes { get; private set; }
    public MaintenanceStatus Status { get; private set; }

    public Apartment? Apartment { get; private set; }
    public Room? Room { get; private set; }

    private MaintenancePeriod() { } // EF Core

    private MaintenancePeriod(DateRange dateRange, MaintenanceCategory category, string description, string? notes = null)
    {
        DateRange = dateRange ?? throw new ArgumentNullException(nameof(dateRange));
        Category = category;
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Notes = notes;
        Status = DetermineStatus(dateRange);

        AddDomainEvent(new MaintenanceScheduledEvent(this));
    }

    public static MaintenancePeriod ForApartment(int apartmentId, DateRange dateRange, MaintenanceCategory category, string description, string? notes = null)
    {
        var maintenance = new MaintenancePeriod(dateRange, category, description, notes);
        maintenance.ApartmentId = apartmentId;
        return maintenance;
    }

    public static MaintenancePeriod ForRoom(int roomId, DateRange dateRange, MaintenanceCategory category, string description, string? notes = null)
    {
        var maintenance = new MaintenancePeriod(dateRange, category, description, notes);
        maintenance.RoomId = roomId;
        return maintenance;
    }

    public void UpdateDateRange(DateRange newDateRange)
    {
        DateRange = newDateRange ?? throw new ArgumentNullException(nameof(newDateRange));
        Status = DetermineStatus(newDateRange);
        ModifiedDate = DateTime.UtcNow;
    }

    public void UpdateDetails(MaintenanceCategory category, string description, string? notes = null)
    {
        Category = category;
        Description = description ?? throw new ArgumentNullException(nameof(description));
        Notes = notes;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = MaintenanceStatus.Completed;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = MaintenanceStatus.Cancelled;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Start()
    {
        Status = MaintenanceStatus.InProgress;
        ModifiedDate = DateTime.UtcNow;
    }

    public string Target => ApartmentId.HasValue
        ? $"Apartment: {Apartment?.Name ?? "Unknown"}"
        : $"Room: {Room?.Apartment?.Name ?? "Unknown"} - {Room?.RoomNumber ?? "Unknown"}";

    public int Duration => DateRange.Duration;

    private static MaintenanceStatus DetermineStatus(DateRange dateRange)
    {
        var today = DateTime.Today;

        if (dateRange.StartDate > today)
            return MaintenanceStatus.Scheduled;

        if (dateRange.Contains(today))
            return MaintenanceStatus.InProgress;

        return MaintenanceStatus.Completed;
    }
}

// GuestRoomAllocation.Domain/Entities/User.cs
using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Enums;
using GuestRoomAllocation.Domain.ValueObjects;

namespace GuestRoomAllocation.Domain.Entities;

public class User : HasDomainEventsEntity
{
    public string Username { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PasswordHash { get; private set; }
    public UserRole Role { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime? LastLoginDate { get; private set; }
    public string? Notes { get; private set; }

    private readonly List<UserApartmentAccess> _apartmentAccess = new();
    public IReadOnlyCollection<UserApartmentAccess> ApartmentAccess => _apartmentAccess.AsReadOnly();

    private readonly List<UserGuestAccess> _guestAccess = new();
    public IReadOnlyCollection<UserGuestAccess> GuestAccess => _guestAccess.AsReadOnly();

    private User() { } // EF Core

    public User(string username, string firstName, string lastName, string email, string passwordHash, UserRole role = UserRole.PropertyManager)
    {
        Username = username ?? throw new ArgumentNullException(nameof(username));
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Role = role;
        IsActive = true;
    }

    public string FullName => $"{FirstName} {LastName}";

    public void UpdateProfile(string firstName, string lastName, string email, string? notes = null)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        Notes = notes;
        ModifiedDate = DateTime.UtcNow;
    }

    public void ChangePassword(string newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
        ModifiedDate = DateTime.UtcNow;
    }

    public void UpdateLastLogin()
    {
        LastLoginDate = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        ModifiedDate = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        ModifiedDate = DateTime.UtcNow;
    }

    public void GrantApartmentAccess(int apartmentId)
    {
        if (!_apartmentAccess.Any(a => a.ApartmentId == apartmentId))
        {
            _apartmentAccess.Add(new UserApartmentAccess(Id, apartmentId));
        }
    }

    public void RevokeApartmentAccess(int apartmentId)
    {
        var access = _apartmentAccess.FirstOrDefault(a => a.ApartmentId == apartmentId);
        if (access != null)
        {
            _apartmentAccess.Remove(access);
        }
    }

    public void GrantGuestAccess(int guestId)
    {
        if (!_guestAccess.Any(g => g.GuestId == guestId))
        {
            _guestAccess.Add(new UserGuestAccess(Id, guestId));
        }
    }

    public void RevokeGuestAccess(int guestId)
    {
        var access = _guestAccess.FirstOrDefault(g => g.GuestId == guestId);
        if (access != null)
        {
            _guestAccess.Remove(access);
        }
    }

    public bool HasApartmentAccess(int apartmentId)
    {
        return Role == UserRole.Admin || _apartmentAccess.Any(a => a.ApartmentId == apartmentId);
    }

    public bool HasGuestAccess(int guestId)
    {
        return Role == UserRole.Admin || _guestAccess.Any(g => g.GuestId == guestId);
    }
}

// GuestRoomAllocation.Domain/Entities/UserApartmentAccess.cs
using GuestRoomAllocation.Domain.Common;

namespace GuestRoomAllocation.Domain.Entities;

public class UserApartmentAccess : BaseEntity
{
    public int UserId { get; private set; }
    public int ApartmentId { get; private set; }
    public DateTime GrantedDate { get; private set; }

    public User User { get; private set; } = null!;
    public Apartment Apartment { get; private set; } = null!;

    private UserApartmentAccess() { } // EF Core

    internal UserApartmentAccess(int userId, int apartmentId)
    {
        UserId = userId;
        ApartmentId = apartmentId;
        GrantedDate = DateTime.UtcNow;
    }
}

// GuestRoomAllocation.Domain/Entities/UserGuestAccess.cs
using GuestRoomAllocation.Domain.Common;

namespace GuestRoomAllocation.Domain.Entities;

public class UserGuestAccess : BaseEntity
{
    public int UserId { get; private set; }
    public int GuestId { get; private set; }
    public DateTime GrantedDate { get; private set; }

    public User User { get; private set; } = null!;
    public Guest Guest { get; private set; } = null!;

    private UserGuestAccess() { } // EF Core

    internal UserGuestAccess(int userId, int guestId)
    {
        UserId = userId;
        GuestId = guestId;
        GrantedDate = DateTime.UtcNow;
    }
}