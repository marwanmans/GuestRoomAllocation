using GuestRoomAllocation.Application.Common.Interfaces;
using GuestRoomAllocation.Infrastructure.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GuestRoomAllocation.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTime, DateTimeService>();
        services.AddScoped<INotificationService, NotificationService>();

        return services;
    }
}