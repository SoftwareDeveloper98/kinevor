using Kinevor.Modules.Notifications.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Kinevor.Modules.Notifications.DependencyInjection;

/// <summary>Registers the notifications application services.</summary>
public static class NotificationsModule
{
    /// <summary>Adds MediatR handlers, the delivery simulator and the system clock.</summary>
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssemblyContaining<GetMyNotificationsHandler>());
        services.AddScoped<INotificationDeliverySimulator, NotificationDeliverySimulator>();
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
