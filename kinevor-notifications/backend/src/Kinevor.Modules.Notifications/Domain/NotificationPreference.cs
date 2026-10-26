using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.Notifications.Domain;

/// <summary>Per-member channel preferences for one notification category.</summary>
public sealed class NotificationPreference : Entity
{
    private NotificationPreference()
    {
    }

    /// <summary>Member the preference belongs to.</summary>
    public Guid MemberId { get; private set; }

    /// <summary>Preference category key.</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Whether e-mail notifications are enabled.</summary>
    public bool EmailEnabled { get; private set; } = true;

    /// <summary>Whether push notifications are enabled.</summary>
    public bool PushEnabled { get; private set; } = true;

    /// <summary>Whether in-app notifications are enabled.</summary>
    public bool InAppEnabled { get; private set; } = true;

    /// <summary>Creates default preferences for a member and category.</summary>
    public static NotificationPreference For(Guid memberId, string category) => new()
    {
        MemberId = memberId,
        Category = NotificationCategory.Normalize(category),
    };

    /// <summary>Sets the three channel flags.</summary>
    public void Update(bool email, bool push, bool inApp)
    {
        EmailEnabled = email;
        PushEnabled = push;
        InAppEnabled = inApp;
    }

    /// <summary>Whether the channel is currently allowed for the category.</summary>
    public bool Allows(NotificationChannel channel) => channel switch
    {
        NotificationChannel.Email => EmailEnabled,
        NotificationChannel.Push => PushEnabled,
        _ => InAppEnabled,
    };
}
