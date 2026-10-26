using Kinevor.Modules.Notifications.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Kinevor.Modules.Notifications.EntityFrameworkCore;

/// <summary>Registers notifications persistence services.</summary>
public static class NotificationsEntityFrameworkCoreExtensions
{
    /// <summary>Adds repositories and the module unit of work.</summary>
    public static IServiceCollection AddNotificationsEntityFrameworkCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
        services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
        services.AddScoped<IEmailSuppressionRepository, EmailSuppressionRepository>();
        services.AddScoped<INotificationTemplateRepository, NotificationTemplateRepository>();
        services.AddScoped<INotificationsUnitOfWork>(provider =>
            provider.GetRequiredService<NotificationsDbContext>());
        return services;
    }
}
