namespace GuestRoomAllocation.Application.Common.Interfaces;

public interface INotificationService
{
    Task SendAllocationConfirmationAsync(int allocationId, CancellationToken cancellationToken = default);
    Task SendMaintenanceNotificationAsync(int maintenancePeriodId, CancellationToken cancellationToken = default);
    Task SendAllocationReminderAsync(int allocationId, CancellationToken cancellationToken = default);
}
