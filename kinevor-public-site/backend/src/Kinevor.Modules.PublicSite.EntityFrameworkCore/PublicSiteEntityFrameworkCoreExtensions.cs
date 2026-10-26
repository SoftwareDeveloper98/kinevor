using Kinevor.Modules.PublicSite.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Kinevor.Modules.PublicSite.EntityFrameworkCore;

/// <summary>Registers public-site persistence services.</summary>
public static class PublicSiteEntityFrameworkCoreExtensions
{
    /// <summary>Adds the repository implementation.</summary>
    public static IServiceCollection AddPublicSiteEntityFrameworkCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IPublicSiteRepository, PublicSiteRepository>();
        return services;
    }
}
