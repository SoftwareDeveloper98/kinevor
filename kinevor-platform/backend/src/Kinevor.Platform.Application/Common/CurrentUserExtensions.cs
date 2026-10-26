namespace Kinevor.Platform.Application.Common;

/// <summary>Role-check helpers for <see cref="ICurrentUser"/>.</summary>
public static class CurrentUserExtensions
{
    /// <summary>Whether the user holds the given role (case-insensitive).</summary>
    public static bool IsInRole(this ICurrentUser user, string role) =>
        user.Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

    /// <summary>Whether the user is an Admin.</summary>
    public static bool IsAdmin(this ICurrentUser user) => user.IsInRole("Admin");

    /// <summary>Whether the user is Staff or Admin.</summary>
    public static bool IsStaffOrAdmin(this ICurrentUser user) =>
        user.IsInRole("Admin") || user.IsInRole("Staff");

    /// <summary>Whether the user is Trainer, Staff or Admin.</summary>
    public static bool IsTrainerOrAbove(this ICurrentUser user) =>
        user.IsStaffOrAdmin() || user.IsInRole("Trainer");

    /// <summary>Trainer role without staff/admin rights, so the caller is subject to object-level checks.</summary>
    public static bool IsTrainerOnly(this ICurrentUser user) =>
        user.IsInRole("Trainer") && !user.IsStaffOrAdmin();
}
