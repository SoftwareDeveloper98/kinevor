using Kinevor.Modules.Notifications.Domain;

namespace Kinevor.Modules.Notifications.Application;

/// <summary>Persistence contract for notification records.</summary>
public interface INotificationRepository
{
    /// <summary>Finds a notification by id.</summary>
    Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lists queued notifications that are due at the given time.</summary>
    Task<IReadOnlyList<Notification>> ListDueAsync(DateTime now, CancellationToken ct = default);

    /// <summary>Lists the member's unread notifications.</summary>
    Task<IReadOnlyList<Notification>> ListUnreadAsync(Guid memberId, CancellationToken ct = default);

    /// <summary>Counts the member's unread notifications.</summary>
    Task<int> CountUnreadAsync(Guid memberId, CancellationToken ct = default);

    /// <summary>Filters and pages notifications for the inbox and staff ledger.</summary>
    Task<(IReadOnlyList<Notification> Items, int Total)> SearchAsync(
        Guid? memberId,
        string? category,
        string? channel,
        string? status,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken ct = default);

    /// <summary>Tracks a new notification.</summary>
    void Add(Notification notification);

    /// <summary>Tracks a batch of new notifications.</summary>
    void AddRange(IEnumerable<Notification> notifications);
}

/// <summary>Persistence contract for per-member category preferences.</summary>
public interface INotificationPreferenceRepository
{
    /// <summary>Lists all stored preferences for a member.</summary>
    Task<IReadOnlyList<NotificationPreference>> ListAsync(Guid memberId, CancellationToken ct = default);

    /// <summary>Finds one preference by member and category.</summary>
    Task<NotificationPreference?> FindAsync(Guid memberId, string category, CancellationToken ct = default);

    /// <summary>Tracks a new preference.</summary>
    void Add(NotificationPreference preference);
}

/// <summary>Persistence contract for push device tokens.</summary>
public interface IDeviceTokenRepository
{
    /// <summary>Lists a member's registered devices.</summary>
    Task<IReadOnlyList<DeviceToken>> ListAsync(Guid memberId, CancellationToken ct = default);

    /// <summary>Finds a device by exact token.</summary>
    Task<DeviceToken?> FindByTokenAsync(string token, CancellationToken ct = default);

    /// <summary>Tracks a new device token.</summary>
    void Add(DeviceToken token);

    /// <summary>Removes a device token.</summary>
    void Remove(DeviceToken token);
}

/// <summary>Persistence contract for suppressed e-mail addresses.</summary>
public interface IEmailSuppressionRepository
{
    /// <summary>Lists suppressions, optionally filtered by address fragment.</summary>
    Task<IReadOnlyList<EmailSuppression>> ListAsync(string? search, CancellationToken ct = default);

    /// <summary>Finds a suppression by normalized address.</summary>
    Task<EmailSuppression?> FindAsync(string email, CancellationToken ct = default);

    /// <summary>Tracks a new suppression.</summary>
    void Add(EmailSuppression suppression);

    /// <summary>Removes a suppression.</summary>
    void Remove(EmailSuppression suppression);
}

/// <summary>Persistence contract for message templates.</summary>
public interface INotificationTemplateRepository
{
    /// <summary>Lists templates ordered by key.</summary>
    Task<IReadOnlyList<NotificationTemplate>> ListAsync(CancellationToken ct = default);

    /// <summary>Finds a template by normalized key.</summary>
    Task<NotificationTemplate?> FindAsync(string key, CancellationToken ct = default);
}

/// <summary>Unit of work owned by the notifications module.</summary>
public interface INotificationsUnitOfWork
{
    /// <summary>Persists pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
