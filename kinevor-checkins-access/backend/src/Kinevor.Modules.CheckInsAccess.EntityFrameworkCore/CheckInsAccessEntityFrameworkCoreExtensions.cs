using Kinevor.Modules.CheckInsAccess.Application;
using Microsoft.Extensions.DependencyInjection;

namespace Kinevor.Modules.CheckInsAccess.EntityFrameworkCore;

/// <summary>Registers check-in persistence services.</summary>
public static class CheckInsAccessEntityFrameworkCoreExtensions
{
    /// <summary>Adds repositories and the module unit of work.</summary>
    /// <param name="services">Host service collection.</param>
    /// <returns>The same collection for chaining.</returns>
    public static IServiceCollection AddCheckInsAccessEntityFrameworkCore(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddScoped<IAccessMemberRepository, AccessMemberRepository>();
        services.AddScoped<IClubBranchRepository, ClubBranchRepository>();
        services.AddScoped<ICheckInRepository, CheckInRepository>();
        services.AddScoped<IAccessAuditRepository, AccessAuditRepository>();
        services.AddScoped<ICheckInsAccessUnitOfWork>(
            provider => provider.GetRequiredService<CheckInsDbContext>());
        return services;
    }
}
