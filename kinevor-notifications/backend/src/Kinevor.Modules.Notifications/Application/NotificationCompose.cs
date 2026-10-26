using Kinevor.Modules.Notifications.Domain;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using MediatR;

namespace Kinevor.Modules.Notifications.Application;

/// <summary>Composes and delivers one notification for a member.</summary>
public record ComposeNotificationCommand(
    Guid MemberId,
    string? TemplateKey = null,
    string? Title = null,
    string? Body = null,
    string? Category = null,
    string? Channel = null,
    string? RecipientEmail = null,
    DateTime? ScheduledAt = null,
    IReadOnlyDictionary<string, string>? Variables = null) : ICommand<SendResultDto>;

/// <summary>Handles <see cref="ComposeNotificationCommand"/>.</summary>
public class ComposeNotificationHandler(
    INotificationRepository notifications,
    INotificationTemplateRepository templates,
    INotificationDeliverySimulator simulator,
    INotificationsUnitOfWork unitOfWork,
    TimeProvider clock) : IRequestHandler<ComposeNotificationCommand, SendResultDto>
{
    /// <summary>Validates the request, applies template defaults and simulates delivery.</summary>
    public async Task<SendResultDto> Handle(ComposeNotificationCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.MemberId == Guid.Empty)
            throw new DomainException("Recipient member is required.");

        var template = await NotificationComposer.FindTemplateAsync(command.TemplateKey, templates, ct);
        var category = NotificationComposer.ResolveCategory(command.Category, template);
        var channel = NotificationComposer.ResolveChannel(command.Channel, template);
        var (title, body) = NotificationComposer.Render(command.Title, command.Body, template, command.Variables);

        var created = Notification.Create(
            command.MemberId,
            title,
            body,
            channel,
            category,
            command.RecipientEmail,
            template?.Key,
            command.ScheduledAt);

        if (created.IsFailure) throw new DomainException(created.Error.Message);

        var notification = created.Value;
        notifications.Add(notification);

        var now = clock.GetUtcNow().UtcDateTime;
        if (!notification.IsDue(now))
        {
            await unitOfWork.SaveChangesAsync(ct);
            return new SendResultDto(1, 0, 0, 1);
        }

        var delivery = await simulator.DeliverAsync(notification, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return delivery.Delivered
            ? new SendResultDto(1, 1, 0, 0)
            : new SendResultDto(1, 0, 1, 0);
    }
}

/// <summary>Composes and delivers the same message for several members.</summary>
public record BroadcastNotificationCommand(
    IReadOnlyList<Guid> MemberIds,
    string? TemplateKey = null,
    string? Title = null,
    string? Body = null,
    string? Category = null,
    string? Channel = null,
    DateTime? ScheduledAt = null,
    IReadOnlyDictionary<string, string>? Variables = null) : ICommand<SendResultDto>;

/// <summary>Handles <see cref="BroadcastNotificationCommand"/>.</summary>
public class BroadcastNotificationHandler(
    INotificationRepository notifications,
    INotificationTemplateRepository templates,
    INotificationDeliverySimulator simulator,
    INotificationsUnitOfWork unitOfWork,
    TimeProvider clock) : IRequestHandler<BroadcastNotificationCommand, SendResultDto>
{
    /// <summary>Validates recipients, creates one record per member and simulates delivery.</summary>
    public async Task<SendResultDto> Handle(BroadcastNotificationCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.MemberIds is null || command.MemberIds.Count == 0)
            throw new DomainException("At least one recipient member is required.");

        var memberIds = command.MemberIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (memberIds.Count == 0)
            throw new DomainException("At least one valid recipient member is required.");

        if (memberIds.Count > 500)
            throw new DomainException("A broadcast may target at most 500 members.");

        var template = await NotificationComposer.FindTemplateAsync(command.TemplateKey, templates, ct);
        var category = NotificationComposer.ResolveCategory(command.Category, template);
        var channel = NotificationComposer.ResolveChannel(command.Channel, template);
        var (title, body) = NotificationComposer.Render(command.Title, command.Body, template, command.Variables);

        var batch = new List<Notification>(memberIds.Count);
        foreach (var memberId in memberIds)
        {
            var created = Notification.Create(
                memberId,
                title,
                body,
                channel,
                category,
                null,
                template?.Key,
                command.ScheduledAt);

            if (created.IsFailure) throw new DomainException(created.Error.Message);
            batch.Add(created.Value);
        }

        notifications.AddRange(batch);

        var now = clock.GetUtcNow().UtcDateTime;
        var delivered = 0;
        var failed = 0;
        var scheduled = 0;

        foreach (var notification in batch)
        {
            if (!notification.IsDue(now))
            {
                scheduled++;
                continue;
            }

            var delivery = await simulator.DeliverAsync(notification, ct);
            if (delivery.Delivered) delivered++;
            else failed++;
        }

        await unitOfWork.SaveChangesAsync(ct);
        return new SendResultDto(batch.Count, delivered, failed, scheduled);
    }
}

/// <summary>Resolves templates, categories and channels shared by compose and broadcast.</summary>
internal static class NotificationComposer
{
    internal static async Task<NotificationTemplate?> FindTemplateAsync(
        string? key,
        INotificationTemplateRepository templates,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(key)) return null;

        return await templates.FindAsync(key, ct)
            ?? throw new DomainException($"Notification template '{key}' was not found.");
    }

    internal static string ResolveCategory(string? requested, NotificationTemplate? template)
    {
        if (!string.IsNullOrWhiteSpace(requested))
        {
            if (!NotificationCategory.IsKnown(requested))
                throw new DomainException($"Unknown notification category '{requested}'.");

            return NotificationCategory.Normalize(requested);
        }

        if (template is not null) return template.Category;

        throw new DomainException("A category is required when no template is supplied.");
    }

    internal static NotificationChannel ResolveChannel(string? requested, NotificationTemplate? template)
    {
        if (string.IsNullOrWhiteSpace(requested))
            return template?.DefaultChannel ?? NotificationChannel.InApp;

        if (Enum.TryParse<NotificationChannel>(requested, true, out var channel) &&
            Enum.IsDefined(channel))
        {
            return channel;
        }

        throw new DomainException(
            $"Channel must be one of: {string.Join(", ", Enum.GetNames<NotificationChannel>()).ToLowerInvariant()}.");
    }

    internal static (string Title, string Body) Render(
        string? requestedTitle,
        string? requestedBody,
        NotificationTemplate? template,
        IReadOnlyDictionary<string, string>? variables)
    {
        var rendered = template?.Render(variables) ?? (string.Empty, string.Empty);
        var title = string.IsNullOrWhiteSpace(requestedTitle) ? rendered.Item1 : requestedTitle;
        var body = string.IsNullOrWhiteSpace(requestedBody) ? rendered.Item2 : requestedBody;
        return (title, body);
    }
}
