using Kinevor.Modules.IdentityAccess.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Kinevor.Modules.IdentityAccess.EntityFrameworkCore;

/// <summary>Registers the identity module persistence services.</summary>
public static class IdentityEntityFrameworkCoreExtensions
{
    /// <summary>Adds repositories and the module unit of work.</summary>
    public static IServiceCollection AddIdentityEntityFrameworkCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IIdentityUnitOfWork>(provider => provider.GetRequiredService<IdentityDbContext>());
        return services;
    }
}
