using Microsoft.AspNetCore.Authorization;

namespace Kinevor.Platform.Api;

/// <summary>Registers the shared Kinevor authorization policies.</summary>
public static class AuthorizationOptionsExtensions
{
    /// <summary>Adds every policy from <see cref="Policies"/> to the options.</summary>
    public static AuthorizationOptions AddKinevorPolicies(this AuthorizationOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        options.AddPolicy(Policies.AdminOnly, p =>
            p.RequireRole(Roles.Admin));
        options.AddPolicy(Policies.StaffOrAdmin, p =>
            p.RequireRole(Roles.Admin, Roles.Staff));
        options.AddPolicy(Policies.TrainerOrAbove, p =>
            p.RequireRole(Roles.Admin, Roles.Staff, Roles.Trainer));
        options.AddPolicy(Policies.MemberOrAbove, p =>
            p.RequireRole(Roles.Admin, Roles.Staff, Roles.Trainer, Roles.Member));
        options.AddPolicy(Policies.Authenticated, p =>
            p.RequireAuthenticatedUser());
        return options;
    }
}
