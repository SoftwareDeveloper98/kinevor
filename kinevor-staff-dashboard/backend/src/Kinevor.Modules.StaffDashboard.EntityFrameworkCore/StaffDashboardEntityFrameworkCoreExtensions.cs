using Kinevor.Modules.StaffDashboard.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Kinevor.Modules.StaffDashboard.EntityFrameworkCore;

/// <summary>Registers staff dashboard persistence services.</summary>
public static class StaffDashboardEntityFrameworkCoreExtensions
{
    /// <summary>Adds the dashboard repository.</summary>
    public static IServiceCollection AddStaffDashboardEntityFrameworkCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IDashboardRepository, DashboardRepository>();
        return services;
    }
}
