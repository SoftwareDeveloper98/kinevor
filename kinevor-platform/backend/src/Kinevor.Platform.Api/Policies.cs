using Microsoft.AspNetCore.Authorization;

namespace Kinevor.Platform.Api;

/// <summary>Central policy names for Kinevor module APIs.</summary>
public static class Policies
{
    /// <summary>Admin-only access.</summary>
    public const string AdminOnly = "AdminOnly";

    /// <summary>Staff and admin access.</summary>
    public const string StaffOrAdmin = "StaffOrAdmin";

    /// <summary>Trainer, staff and admin access.</summary>
    public const string TrainerOrAbove = "TrainerOrAbove";

    /// <summary>Any authenticated role including members.</summary>
    public const string MemberOrAbove = "MemberOrAbove";

    /// <summary>Any authenticated user.</summary>
    public const string Authenticated = "Authenticated";
}
