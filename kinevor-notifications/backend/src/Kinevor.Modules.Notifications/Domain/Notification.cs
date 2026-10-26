using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.Notifications.Domain;

/// <summary>
/// One notification record addressed to an external member id. Staff create records as
/// <see cref="NotificationStatus.Queued"/>; the delivery simulation transitions them to
/// <see cref="NotificationStatus.Sent"/> or <see cref="NotificationStatus.Failed"/>, and the
/// in-app inbox transitions delivered records to <see cref="NotificationStatus.Read"/>.
/// </summary>
public sealed class Notification : AggregateRoot
{
    /// <summary>Maximum accepted title length.</summary>
    public const int TitleMaxLength = 160;

    /// <summary>Maximum accepted body length.</summary>
    public const int BodyMaxLength = 2000;

    private Notification()
    {
    }

    /// <summary>External member id owned by the members module.</summary>
    public Guid MemberId { get; private set; }

    /// <summary>Notification title.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>Notification body.</summary>
    public string Body { get; private set; } = string.Empty;

    /// <summary>Channel the record is delivered through.</summary>
    public NotificationChannel Channel { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public NotificationStatus Status { get; private set; } = NotificationStatus.Queued;

    /// <summary>Preference category controlling this notification.</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Recipient e-mail snapshot used for suppression checks on the e-mail channel.</summary>
    public string? RecipientEmail { get; private set; }

    /// <summary>Template key the message was rendered from, when any.</summary>
    public string? TemplateKey { get; private set; }

    /// <summary>Planned delivery time; <see langword="null"/> means deliver immediately.</summary>
    public DateTime? ScheduledAt { get; private set; }

    /// <summary>When the delivery simulation accepted the record.</summary>
    public DateTime? SentAt { get; private set; }

    /// <summary>When the recipient opened the record in the in-app inbox.</summary>
    public DateTime? ReadAt { get; private set; }

    /// <summary>Why delivery failed, when it did.</summary>
    public string? FailureReason { get; private set; }

    /// <summary>Whether the record is waiting to be read by the recipient.</summary>
    public bool IsUnread => Status is NotificationStatus.Queued or NotificationStatus.Sent;

    /// <summary>Whether the record is due for delivery at <paramref name="now"/>.</summary>
    public bool IsDue(DateTime now) =>
        Status == NotificationStatus.Queued && (ScheduledAt is null || ScheduledAt <= now);

    /// <summary>Validates and creates a queued notification record.</summary>
    public static Result<Notification> Create(
        Guid memberId,
        string title,
        string body,
        NotificationChannel channel,
        string category,
        string? recipientEmail = null,
        string? templateKey = null,
        DateTime? scheduledAt = null)
    {
        if (memberId == Guid.Empty)
            return Result.Failure<Notification>(Error.Validation("MemberId", "Recipient member is required."));

        var normalizedTitle = (title ?? string.Empty).Trim();
        if (normalizedTitle.Length == 0)
            return Result.Failure<Notification>(Error.Validation("Title", "Title is required."));
        if (normalizedTitle.Length > TitleMaxLength)
            return Result.Failure<Notification>(Error.Validation("Title", $"Title must be at most {TitleMaxLength} characters."));

        var normalizedBody = (body ?? string.Empty).Trim();
        if (normalizedBody.Length == 0)
            return Result.Failure<Notification>(Error.Validation("Body", "Body is required."));
        if (normalizedBody.Length > BodyMaxLength)
            return Result.Failure<Notification>(Error.Validation("Body", $"Body must be at most {BodyMaxLength} characters."));

        if (!NotificationCategory.IsKnown(category))
            return Result.Failure<Notification>(Error.Validation("Category",
                $"Unknown notification category '{category}'."));

        var email = string.IsNullOrWhiteSpace(recipientEmail) ? null : recipientEmail.Trim().ToLowerInvariant();

        return Result.Success(new Notification
        {
            MemberId = memberId,
            Title = normalizedTitle,
            Body = normalizedBody,
            Channel = channel,
            Category = NotificationCategory.Normalize(category),
            RecipientEmail = email,
            TemplateKey = string.IsNullOrWhiteSpace(templateKey) ? null : templateKey.Trim(),
            ScheduledAt = scheduledAt,
        });
    }

    /// <summary>Marks a queued record as delivered.</summary>
    public Result MarkSent(DateTime now)
    {
        if (Status != NotificationStatus.Queued)
            return Result.Failure(Error.Business("Notification.NotQueued",
                "Only queued notifications can be sent."));

        Status = NotificationStatus.Sent;
        SentAt = now;
        FailureReason = null;
        return Result.Success();
    }

    /// <summary>Marks a queued record as failed with a reason.</summary>
    public Result MarkFailed(string reason, DateTime now)
    {
        if (Status != NotificationStatus.Queued)
            return Result.Failure(Error.Business("Notification.NotQueued",
                "Only queued notifications can fail."));

        if (string.IsNullOrWhiteSpace(reason))
            return Result.Failure(Error.Validation("FailureReason", "Failure reason is required."));

        Status = NotificationStatus.Failed;
        SentAt = null;
        FailureReason = reason.Trim();
        return Result.Success();
    }

    /// <summary>Marks a delivered record as read; reading a read record is a no-op.</summary>
    public Result MarkRead(DateTime now)
    {
        if (Status == NotificationStatus.Read)
            return Result.Success();

        if (Status != NotificationStatus.Sent)
            return Result.Failure(Error.Business("Notification.NotDelivered",
                "Only delivered notifications can be marked read."));

        Status = NotificationStatus.Read;
        ReadAt = now;
        return Result.Success();
    }
}
