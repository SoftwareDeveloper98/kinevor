using Kinevor.Modules.StaffDashboard.Application;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Kinevor.Modules.StaffDashboard.DependencyInjection;

/// <summary>Registers staff dashboard application services.</summary>
public static class StaffDashboardModule
{
    /// <summary>Adds the dashboard service and system clock.</summary>
    public static IServiceCollection AddStaffDashboardModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<StaffDashboardService>();
        services.TryAddSingleton(TimeProvider.System);
        return services;
    }
}
