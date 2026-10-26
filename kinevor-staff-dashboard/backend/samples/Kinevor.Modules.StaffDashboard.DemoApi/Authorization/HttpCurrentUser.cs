using System.Security.Claims;
using Kinevor.Platform.Application.Common;

namespace Kinevor.Modules.StaffDashboard.DemoApi.Authorization;

/// <summary>Reads the current demo user from the authenticated principal.</summary>
public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal User => accessor.HttpContext?.User ?? new ClaimsPrincipal();

    /// <summary>Identity user id parsed from the name identifier claim.</summary>
    public Guid? UserId => Parse(ClaimTypes.NameIdentifier);

    /// <summary>Roles carried by the principal.</summary>
    public IReadOnlyList<string> Roles =>
        User.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray();

    /// <summary>E-mail address carried by the principal.</summary>
    public string? Email => User.FindFirstValue(ClaimTypes.Email);

    /// <summary>Home branch claim.</summary>
    public Guid? BranchId => Parse("branch_id");

    /// <summary>The demo does not issue member claims.</summary>
    public Guid? MemberId => null;

    /// <summary>The demo does not issue trainer claims.</summary>
    public Guid? TrainerId => null;

    private Guid? Parse(string claimType) =>
        Guid.TryParse(User.FindFirstValue(claimType), out var value) ? value : null;
}
