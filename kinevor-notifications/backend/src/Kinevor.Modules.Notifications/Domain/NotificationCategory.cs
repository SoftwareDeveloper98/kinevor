namespace Kinevor.Modules.Notifications.Domain;

/// <summary>Well-known notification categories shared with member preferences.</summary>
public static class NotificationCategory
{
    /// <summary>Category key for class reminders and booking updates.</summary>
    public const string ClassReminders = "class_reminders";

    /// <summary>Category key for membership expiry and renewal reminders.</summary>
    public const string MembershipExpiry = "membership_expiry";

    /// <summary>Category key for marketing offers.</summary>
    public const string Offers = "offers";

    /// <summary>Category key for payment receipts.</summary>
    public const string Receipts = "receipts";

    /// <summary>Every category members can configure.</summary>
    public static readonly string[] All = [ClassReminders, MembershipExpiry, Offers, Receipts];

    /// <summary>Whether a category key is one of the well-known categories.</summary>
    public static bool IsKnown(string? category) =>
        !string.IsNullOrWhiteSpace(category) &&
        All.Contains(Normalize(category));

    /// <summary>Trims and lower-cases a category key.</summary>
    public static string Normalize(string category) => category.Trim().ToLowerInvariant();
}
