using Kinevor.Modules.PublicSite.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Kinevor.Modules.PublicSite.DependencyInjection;

/// <summary>Registers the public-site application services.</summary>
public static class PublicSiteModule
{
    /// <summary>Adds MediatR handlers and the system clock.</summary>
    public static IServiceCollection AddPublicSiteModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(configuration =>
            configuration.RegisterServicesFromAssemblyContaining<GetPublicSiteQuery>());
        services.AddSingleton(TimeProvider.System);
        return services;
    }
}
