using GuestRoomAllocation.Domain.Common;
using GuestRoomAllocation.Domain.Enums;
using GuestRoomAllocation.Domain.Events;
using GuestRoomAllocation.Domain.ValueObjects;

namespace GuestRoomAllocation.Domain.Entities;

public class MaintenancePeriod : HasDomainEventsEntity
{
    public int? ApartmentId { get; private set; }
    public int? RoomId { get; private set; }
    public DateRange DateRange { get; private set; } = null!;
    public MaintenanceCategory Category { get; private set; }
    public string Description { get; private set; } = string.Empty;
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

    public void Complete()
    {
        Status = MaintenanceStatus.Completed;
        ModifiedDate = DateTime.UtcNow;
    }

    public string Target => ApartmentId.HasValue
        ? $"Apartment: {Apartment?.Name ?? "Unknown"}"
        : $"Room: {Room?.Apartment?.Name ?? "Unknown"} - {Room?.RoomNumber ?? "Unknown"}";

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