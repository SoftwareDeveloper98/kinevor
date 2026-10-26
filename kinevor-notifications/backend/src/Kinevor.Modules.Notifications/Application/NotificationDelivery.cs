using Kinevor.Modules.Notifications.Domain;
using Kinevor.Platform.Application.Common;
using MediatR;

namespace Kinevor.Modules.Notifications.Application;

/// <summary>
/// Simulates the external delivery providers. No e-mail, push or SMS provider is ever called:
/// the simulator applies member preferences, registered devices and the suppression list, then
/// records the resulting status on the notification.
/// </summary>
public interface INotificationDeliverySimulator
{
    /// <summary>Attempts delivery for one queued notification.</summary>
    Task<NotificationDelivery> DeliverAsync(Notification notification, CancellationToken ct = default);
}

/// <summary>Default preference-, device- and suppression-aware delivery simulator.</summary>
public sealed class NotificationDeliverySimulator(
    INotificationPreferenceRepository preferences,
    IDeviceTokenRepository devices,
    IEmailSuppressionRepository suppressions,
    TimeProvider clock) : INotificationDeliverySimulator
{
    /// <summary>Applies the delivery rules and records sent or failed on the notification.</summary>
    public async Task<NotificationDelivery> DeliverAsync(Notification notification, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(notification);

        var now = clock.GetUtcNow().UtcDateTime;
        var preference = await preferences.FindAsync(notification.MemberId, notification.Category, ct);
        if (preference is not null && !preference.Allows(notification.Channel))
            return Fail(notification, $"Disabled by member preference ({notification.Category}/{ChannelName(notification.Channel)}).", now);

        switch (notification.Channel)
        {
            case NotificationChannel.Email:
                if (string.IsNullOrWhiteSpace(notification.RecipientEmail))
                    return Fail(notification, "No recipient e-mail address was provided for the e-mail channel.", now);

                var suppression = await suppressions.FindAsync(notification.RecipientEmail, ct);
                if (suppression is not null)
                    return Fail(notification, $"Recipient e-mail is suppressed ({suppression.Reason}).", now);
                break;

            case NotificationChannel.Push:
                var registered = await devices.ListAsync(notification.MemberId, ct);
                if (registered.Count == 0)
                    return Fail(notification, "No registered push device for the member.", now);
                break;

            default:
                break;
        }

        var sent = notification.MarkSent(now);
        return sent.IsFailure
            ? new NotificationDelivery(notification.Id, false, sent.Error.Message)
            : new NotificationDelivery(notification.Id, true, null);
    }

    private static NotificationDelivery Fail(Notification notification, string reason, DateTime now)
    {
        var failed = notification.MarkFailed(reason, now);
        return failed.IsFailure
            ? new NotificationDelivery(notification.Id, false, failed.Error.Message)
            : new NotificationDelivery(notification.Id, false, reason);
    }

    private static string ChannelName(NotificationChannel channel) => channel switch
    {
        NotificationChannel.Email => "email",
        NotificationChannel.Push => "push",
        _ => "in_app",
    };
}

/// <summary>Processes every queued notification that is due, using the simulated providers.</summary>
public record DispatchDueNotificationsCommand : ICommand<DispatchResultDto>;

/// <summary>Handles <see cref="DispatchDueNotificationsCommand"/>.</summary>
public class DispatchDueNotificationsHandler(
    INotificationRepository notifications,
    INotificationDeliverySimulator simulator,
    INotificationsUnitOfWork unitOfWork,
    TimeProvider clock) : IRequestHandler<DispatchDueNotificationsCommand, DispatchResultDto>
{
    /// <summary>Delivers due notifications and persists the resulting statuses.</summary>
    public async Task<DispatchResultDto> Handle(DispatchDueNotificationsCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        var due = await notifications.ListDueAsync(clock.GetUtcNow().UtcDateTime, ct);
        var delivered = 0;

        foreach (var notification in due)
        {
            var result = await simulator.DeliverAsync(notification, ct);
            if (result.Delivered) delivered++;
        }

        await unitOfWork.SaveChangesAsync(ct);
        return new DispatchResultDto(due.Count, delivered, due.Count - delivered);
    }
}
