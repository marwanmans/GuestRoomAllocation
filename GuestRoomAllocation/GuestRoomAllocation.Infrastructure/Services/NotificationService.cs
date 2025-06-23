using GuestRoomAllocation.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace GuestRoomAllocation.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(ILogger<NotificationService> logger)
    {
        _logger = logger;
    }

    public async Task SendAllocationConfirmationAsync(int allocationId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement email/SMS notification logic
        _logger.LogInformation("Allocation confirmation notification sent for allocation {AllocationId}", allocationId);
        await Task.CompletedTask;
    }

    public async Task SendMaintenanceNotificationAsync(int maintenancePeriodId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement email/SMS notification logic
        _logger.LogInformation("Maintenance notification sent for maintenance period {MaintenancePeriodId}", maintenancePeriodId);
        await Task.CompletedTask;
    }

    public async Task SendAllocationReminderAsync(int allocationId, CancellationToken cancellationToken = default)
    {
        // TODO: Implement email/SMS notification logic
        _logger.LogInformation("Allocation reminder sent for allocation {AllocationId}", allocationId);
        await Task.CompletedTask;
    }
}