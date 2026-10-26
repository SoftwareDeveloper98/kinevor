using Kinevor.Modules.Notifications.Application;
using Kinevor.Modules.Notifications.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kinevor.Modules.Notifications.EntityFrameworkCore;

/// <summary>EF Core notification repository.</summary>
public sealed class NotificationRepository(NotificationsDbContext db) : INotificationRepository
{
    /// <summary>Finds a notification by id.</summary>
    public Task<Notification?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Notifications.FirstOrDefaultAsync(notification => notification.Id == id, ct);

    /// <summary>Lists queued notifications that are due at the given time.</summary>
    public async Task<IReadOnlyList<Notification>> ListDueAsync(
        DateTime now, CancellationToken ct = default) =>
        await db.Notifications
            .Where(notification => notification.Status == NotificationStatus.Queued &&
                (notification.ScheduledAt == null || notification.ScheduledAt <= now))
            .OrderBy(notification => notification.ScheduledAt)
            .ToListAsync(ct);

    /// <summary>Lists the member's delivered but unread notifications.</summary>
    public async Task<IReadOnlyList<Notification>> ListUnreadAsync(
        Guid memberId, CancellationToken ct = default) =>
        await db.Notifications
            .Where(notification => notification.MemberId == memberId &&
                notification.Status == NotificationStatus.Sent)
            .OrderByDescending(notification => notification.CreatedAt)
            .ToListAsync(ct);

    /// <summary>Counts the member's delivered but unread notifications.</summary>
    public Task<int> CountUnreadAsync(Guid memberId, CancellationToken ct = default) =>
        db.Notifications.CountAsync(
            notification => notification.MemberId == memberId &&
                notification.Status == NotificationStatus.Sent,
            ct);

    /// <summary>Filters and pages notifications for the inbox and staff ledger.</summary>
    public async Task<(IReadOnlyList<Notification> Items, int Total)> SearchAsync(
        Guid? memberId,
        string? category,
        string? channel,
        string? status,
        bool unreadOnly,
        int page,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = db.Notifications.AsNoTracking().AsQueryable();

        if (memberId is { } id)
            query = query.Where(notification => notification.MemberId == id);

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalized = NotificationCategory.Normalize(category);
            query = query.Where(notification => notification.Category == normalized);
        }

        if (!string.IsNullOrWhiteSpace(channel))
        {
            query = Enum.TryParse<NotificationChannel>(channel, true, out var parsed) &&
                Enum.IsDefined(parsed)
                ? query.Where(notification => notification.Channel == parsed)
                : query.Where(_ => false);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = Enum.TryParse<NotificationStatus>(status, true, out var parsed) &&
                Enum.IsDefined(parsed)
                ? query.Where(notification => notification.Status == parsed)
                : query.Where(_ => false);
        }

        if (unreadOnly)
        {
            query = query.Where(notification =>
                notification.Status == NotificationStatus.Queued ||
                notification.Status == NotificationStatus.Sent);
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(notification => notification.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return (items, total);
    }

    /// <summary>Tracks a new notification.</summary>
    public void Add(Notification notification) => db.Notifications.Add(notification);

    /// <summary>Tracks a batch of new notifications.</summary>
    public void AddRange(IEnumerable<Notification> notifications) => db.Notifications.AddRange(notifications);
}

/// <summary>EF Core notification preference repository.</summary>
public sealed class NotificationPreferenceRepository(NotificationsDbContext db) : INotificationPreferenceRepository
{
    /// <summary>Lists all stored preferences for a member.</summary>
    public async Task<IReadOnlyList<NotificationPreference>> ListAsync(
        Guid memberId, CancellationToken ct = default) =>
        await db.NotificationPreferences.AsNoTracking()
            .Where(preference => preference.MemberId == memberId)
            .OrderBy(preference => preference.Category)
            .ToListAsync(ct);

    /// <summary>Finds one preference by member and category.</summary>
    public Task<NotificationPreference?> FindAsync(
        Guid memberId, string category, CancellationToken ct = default)
    {
        var normalized = NotificationCategory.Normalize(category);
        return db.NotificationPreferences.FirstOrDefaultAsync(
            preference => preference.MemberId == memberId && preference.Category == normalized,
            ct);
    }

    /// <summary>Tracks a new preference.</summary>
    public void Add(NotificationPreference preference) => db.NotificationPreferences.Add(preference);
}

/// <summary>EF Core device token repository.</summary>
public sealed class DeviceTokenRepository(NotificationsDbContext db) : IDeviceTokenRepository
{
    /// <summary>Lists a member's registered devices.</summary>
    public async Task<IReadOnlyList<DeviceToken>> ListAsync(
        Guid memberId, CancellationToken ct = default) =>
        await db.DeviceTokens
            .Where(device => device.MemberId == memberId)
            .OrderByDescending(device => device.CreatedAt)
            .ToListAsync(ct);

    /// <summary>Finds a device by exact token.</summary>
    public Task<DeviceToken?> FindByTokenAsync(string token, CancellationToken ct = default) =>
        db.DeviceTokens.FirstOrDefaultAsync(device => device.Token == token, ct);

    /// <summary>Tracks a new device token.</summary>
    public void Add(DeviceToken token) => db.DeviceTokens.Add(token);

    /// <summary>Removes a device token.</summary>
    public void Remove(DeviceToken token) => db.DeviceTokens.Remove(token);
}

/// <summary>EF Core e-mail suppression repository.</summary>
public sealed class EmailSuppressionRepository(NotificationsDbContext db) : IEmailSuppressionRepository
{
    /// <summary>Lists suppressions, optionally filtered by address fragment.</summary>
    public async Task<IReadOnlyList<EmailSuppression>> ListAsync(
        string? search, CancellationToken ct = default)
    {
        var query = db.EmailSuppressions.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var normalized = EmailSuppression.NormalizeEmail(search);
            query = query.Where(suppression => suppression.Email.Contains(normalized));
        }

        return await query.OrderBy(suppression => suppression.Email).ToListAsync(ct);
    }

    /// <summary>Finds a suppression by normalized address.</summary>
    public Task<EmailSuppression?> FindAsync(string email, CancellationToken ct = default)
    {
        var normalized = EmailSuppression.NormalizeEmail(email);
        return db.EmailSuppressions.FirstOrDefaultAsync(suppression => suppression.Email == normalized, ct);
    }

    /// <summary>Tracks a new suppression.</summary>
    public void Add(EmailSuppression suppression) => db.EmailSuppressions.Add(suppression);

    /// <summary>Removes a suppression.</summary>
    public void Remove(EmailSuppression suppression) => db.EmailSuppressions.Remove(suppression);
}

/// <summary>EF Core notification template repository.</summary>
public sealed class NotificationTemplateRepository(NotificationsDbContext db) : INotificationTemplateRepository
{
    /// <summary>Lists templates ordered by key.</summary>
    public async Task<IReadOnlyList<NotificationTemplate>> ListAsync(CancellationToken ct = default) =>
        await db.NotificationTemplates.AsNoTracking()
            .OrderBy(template => template.Key)
            .ToListAsync(ct);

    /// <summary>Finds a template by normalized key.</summary>
    public Task<NotificationTemplate?> FindAsync(string key, CancellationToken ct = default)
    {
        var normalized = (key ?? string.Empty).Trim().ToLowerInvariant();
        return db.NotificationTemplates.FirstOrDefaultAsync(template => template.Key == normalized, ct);
    }
}
