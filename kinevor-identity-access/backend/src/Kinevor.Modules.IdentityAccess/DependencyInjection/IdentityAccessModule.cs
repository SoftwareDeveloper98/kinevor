using Kinevor.Modules.IdentityAccess.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Kinevor.Modules.IdentityAccess.DependencyInjection;

/// <summary>Registers the identity and access application services.</summary>
public static class IdentityAccessModule
{
    /// <summary>Adds MediatR handlers, the password hasher, token service and clock.</summary>
    public static IServiceCollection AddIdentityAccessModule(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<LoginHandler>());
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IAuthTokenService, JwtAuthTokenService>();
        return services;
    }
}
