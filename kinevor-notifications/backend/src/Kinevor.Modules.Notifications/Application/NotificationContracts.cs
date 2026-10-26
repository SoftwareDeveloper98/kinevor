using Kinevor.Modules.Notifications.Domain;

namespace Kinevor.Modules.Notifications.Application;

/// <summary>Notification projection shared by the member inbox and the staff ledger.</summary>
public record NotificationDto(
    Guid Id,
    Guid MemberId,
    string Title,
    string Body,
    string Channel,
    string Status,
    string Category,
    string? RecipientEmail,
    string? TemplateKey,
    DateTime? ScheduledAt,
    DateTime? SentAt,
    DateTime? ReadAt,
    string? FailureReason,
    bool Unread,
    DateTime CreatedAt);

/// <summary>One category's channel flags.</summary>
public record NotificationPreferenceDto(string Category, bool Email, bool Push, bool InApp);

/// <summary>Registered push device projection.</summary>
public record DeviceTokenDto(Guid Id, string Platform, string TokenPreview, DateTime RegisteredAt);

/// <summary>Suppressed e-mail address projection.</summary>
public record EmailSuppressionDto(string Email, string Reason, string? Detail, DateTime CreatedAt);

/// <summary>Reusable message template projection.</summary>
public record NotificationTemplateDto(
    string Key,
    string Name,
    string Category,
    string Channel,
    string TitleTemplate,
    string BodyTemplate);

/// <summary>Unread count for the member inbox.</summary>
public record UnreadCountDto(int Unread);

/// <summary>Outcome of a mark-all-read request.</summary>
public record ReadAllResultDto(int Marked);

/// <summary>Outcome of a staff send or broadcast request.</summary>
public record SendResultDto(int Created, int Delivered, int Failed, int Scheduled);

/// <summary>Outcome of a delivery-queue dispatch run.</summary>
public record DispatchResultDto(int Processed, int Delivered, int Failed);

/// <summary>Outcome of simulating delivery for a single notification.</summary>
public record NotificationDelivery(Guid NotificationId, bool Delivered, string? FailureReason);

/// <summary>Maps domain records to API projections.</summary>
internal static class NotificationMapper
{
    internal static NotificationDto ToDto(Notification notification) => new(
        notification.Id,
        notification.MemberId,
        notification.Title,
        notification.Body,
        notification.Channel.ToString(),
        notification.Status.ToString(),
        notification.Category,
        MaskEmail(notification.RecipientEmail),
        notification.TemplateKey,
        notification.ScheduledAt,
        notification.SentAt,
        notification.ReadAt,
        notification.FailureReason,
        notification.IsUnread,
        notification.CreatedAt);

    internal static NotificationPreferenceDto ToDto(NotificationPreference preference) => new(
        preference.Category,
        preference.EmailEnabled,
        preference.PushEnabled,
        preference.InAppEnabled);

    internal static DeviceTokenDto ToDto(DeviceToken token) => new(
        token.Id,
        token.Platform,
        MaskToken(token.Token),
        token.CreatedAt);

    internal static EmailSuppressionDto ToDto(EmailSuppression suppression) => new(
        suppression.Email,
        suppression.Reason,
        suppression.Detail,
        suppression.CreatedAt);

    internal static NotificationTemplateDto ToDto(NotificationTemplate template) => new(
        template.Key,
        template.Name,
        template.Category,
        template.DefaultChannel.ToString(),
        template.TitleTemplate,
        template.BodyTemplate);

    private static string? MaskEmail(string? email)
    {
        if (string.IsNullOrEmpty(email)) return null;
        var at = email.IndexOf('@');
        if (at <= 1) return "***";
        return $"{email[0]}***{email[at..]}";
    }

    private static string MaskToken(string token) =>
        token.Length <= 10 ? "***" : $"{token[..6]}...{token[^4..]}";
}
