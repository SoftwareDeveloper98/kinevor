using Kinevor.Modules.CheckInsAccess.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Kinevor.Modules.CheckInsAccess.DependencyInjection;

/// <summary>Registers check-in application services.</summary>
public static class CheckInsAccessModule
{
    /// <summary>Adds MediatR handlers, the workflow and the system clock.</summary>
    /// <param name="services">Host service collection.</param>
    /// <returns>The same collection for chaining.</returns>
    public static IServiceCollection AddCheckInsAccessModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssemblyContaining<GetAccessMembersHandler>());
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<CheckInWorkflow>();
        return services;
    }
}
