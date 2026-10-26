using Kinevor.Modules.Notifications.Domain;
using Kinevor.Modules.Notifications.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Kinevor.Modules.Notifications.DemoApi.Data;

/// <summary>Seeds deterministic notifications, preferences, devices, templates and suppressions.</summary>
public static class DemoDataSeeder
{
    /// <summary>Id of Maya Thompson, aligned with the members module demo data.</summary>
    public static readonly Guid PrimaryMemberId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    /// <summary>Id of Daniel Okafor, aligned with the members module demo data.</summary>
    public static readonly Guid SecondaryMemberId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    /// <summary>Id of Sofia Rossi, aligned with the members module demo data.</summary>
    public static readonly Guid TertiaryMemberId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    /// <summary>Id of Maya's seeded unread notification.</summary>
    public static readonly Guid UnreadNotificationId = Guid.Parse("70000000-0000-0000-0000-000000000001");

    /// <summary>Id of Maya's seeded read notification.</summary>
    public static readonly Guid ReadNotificationId = Guid.Parse("70000000-0000-0000-0000-000000000002");

    /// <summary>Id of the seeded notification blocked by an e-mail suppression.</summary>
    public static readonly Guid FailedNotificationId = Guid.Parse("70000000-0000-0000-0000-000000000007");

    /// <summary>Id of Maya's seeded device token.</summary>
    public static readonly Guid PrimaryDeviceId = Guid.Parse("80000000-0000-0000-0000-000000000001");

    /// <summary>Adds demo data when the database is empty.</summary>
    public static async Task SeedAsync(NotificationsDbContext db, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(db);

        if (await db.Notifications.AnyAsync(ct))
            return;

        var now = DateTime.UtcNow;

        var templateSeeds = new[]
        {
            new TemplateSeed(
                "booking_confirmed",
                "Booking confirmation",
                NotificationCategory.ClassReminders,
                NotificationChannel.Email,
                "Booking confirmed: {ClassName}",
                "Hi {FirstName}, your spot in {ClassName} on {When} is confirmed."),
            new TemplateSeed(
                "class_reminder",
                "Class reminder",
                NotificationCategory.ClassReminders,
                NotificationChannel.InApp,
                "Class reminder: {ClassName}",
                "Hi {FirstName}, {ClassName} starts at {When}."),
            new TemplateSeed(
                "membership_expiry_reminder",
                "Membership expiry reminder",
                NotificationCategory.MembershipExpiry,
                NotificationChannel.Email,
                "Your membership expires soon",
                "Hi {FirstName}, your membership expires on {ExpiryDate}. Renew to keep access."),
            new TemplateSeed(
                "offer_announcement",
                "Offer announcement",
                NotificationCategory.Offers,
                NotificationChannel.InApp,
                "{OfferTitle}",
                "Hi {FirstName}, {OfferBody}"),
            new TemplateSeed(
                "payment_receipt",
                "Payment receipt",
                NotificationCategory.Receipts,
                NotificationChannel.Email,
                "Receipt {ReceiptNumber}",
                "Hi {FirstName}, we received {Amount}. Thank you."),
        };

        foreach (var seed in templateSeeds)
        {
            var created = NotificationTemplate.Create(
                seed.Key,
                seed.Name,
                seed.Category,
                seed.Channel,
                seed.TitleTemplate,
                seed.BodyTemplate);

            if (created.IsFailure)
                throw new InvalidOperationException(created.Error.Message);

            db.NotificationTemplates.Add(created.Value);
        }

        AddPreference(db, PrimaryMemberId, NotificationCategory.Offers, email: false, push: true, inApp: true);
        AddPreference(db, PrimaryMemberId, NotificationCategory.Receipts, email: true, push: false, inApp: true);
        AddPreference(db, SecondaryMemberId, NotificationCategory.ClassReminders, email: true, push: false, inApp: true);

        var primaryDevice = DeviceToken.Register(
            PrimaryMemberId, "demo-ios-device-token-000000000001", "ios");
        if (primaryDevice.IsFailure)
            throw new InvalidOperationException(primaryDevice.Error.Message);

        db.DeviceTokens.Add(primaryDevice.Value);
        db.Entry(primaryDevice.Value).Property(device => device.Id).CurrentValue = PrimaryDeviceId;

        var secondaryDevice = DeviceToken.Register(
            SecondaryMemberId, "demo-android-device-token-000000000002", "android");
        if (secondaryDevice.IsFailure)
            throw new InvalidOperationException(secondaryDevice.Error.Message);

        db.DeviceTokens.Add(secondaryDevice.Value);

        AddSuppression(db, "bounced@kinevor.demo", "bounce", "Mailbox does not exist.");
        AddSuppression(db, "unsubscribed@kinevor.demo", "unsubscribe", "Member opted out of marketing e-mail.");

        var notificationSeeds = new[]
        {
            new NotificationSeed(
                UnreadNotificationId,
                PrimaryMemberId,
                "Class reminder: Strength Circuit",
                "Hi Maya, your Strength Circuit class starts tomorrow at 18:30.",
                NotificationChannel.InApp,
                NotificationCategory.ClassReminders,
                null,
                null,
                24,
                null,
                24),
            new NotificationSeed(
                ReadNotificationId,
                PrimaryMemberId,
                "Membership expiring soon",
                "Hi Maya, your membership expires in 5 days. Renew to keep your access.",
                NotificationChannel.InApp,
                NotificationCategory.MembershipExpiry,
                null,
                null,
                144,
                120,
                144),
            new NotificationSeed(
                Guid.Parse("70000000-0000-0000-0000-000000000003"),
                PrimaryMemberId,
                "Member-only offer: 20% off personal training",
                "Book a personal training block before Sunday and save 20%.",
                NotificationChannel.InApp,
                NotificationCategory.Offers,
                null,
                "offer_announcement",
                48,
                null,
                48),
            new NotificationSeed(
                Guid.Parse("70000000-0000-0000-0000-000000000004"),
                PrimaryMemberId,
                "Receipt for October membership",
                "Hi Maya, we received GBP 49.00 for your October membership.",
                NotificationChannel.Email,
                NotificationCategory.Receipts,
                "maya.thompson@kinevor.demo",
                "payment_receipt",
                192,
                null,
                192),
            new NotificationSeed(
                Guid.Parse("70000000-0000-0000-0000-000000000005"),
                PrimaryMemberId,
                "Class starts in one hour",
                "Your Strength Circuit class starts at 18:30. See you soon.",
                NotificationChannel.Push,
                NotificationCategory.ClassReminders,
                null,
                null,
                6,
                null,
                6),
            new NotificationSeed(
                Guid.Parse("70000000-0000-0000-0000-000000000006"),
                SecondaryMemberId,
                "Weekend bootcamp",
                "Hi Daniel, the Saturday bootcamp has two places left.",
                NotificationChannel.InApp,
                NotificationCategory.Offers,
                null,
                null,
                24,
                null,
                24),
            new NotificationSeed(
                FailedNotificationId,
                TertiaryMemberId,
                "Receipt for October membership",
                "Hi Sofia, we received GBP 39.00 for your October membership.",
                NotificationChannel.Email,
                NotificationCategory.Receipts,
                "bounced@kinevor.demo",
                "payment_receipt",
                72,
                null,
                72,
                "Recipient e-mail is suppressed (bounce)."),
        };

        var seededNotifications = new List<(Notification Entity, int AgeHours)>();
        foreach (var seed in notificationSeeds)
        {
            var notification = BuildNotification(seed, now);
            db.Notifications.Add(notification);
            db.Entry(notification).Property(item => item.Id).CurrentValue = seed.Id;
            seededNotifications.Add((notification, seed.AgeHours));
        }

        await db.SaveChangesAsync(ct);

        foreach (var (entity, ageHours) in seededNotifications)
        {
            var createdAt = db.Entry(entity).Property(item => item.CreatedAt);
            createdAt.CurrentValue = now.AddHours(-ageHours);
            createdAt.IsModified = true;
        }

        await db.SaveChangesAsync(ct);
    }

    private static Notification BuildNotification(NotificationSeed seed, DateTime now)
    {
        var created = Notification.Create(
            seed.MemberId,
            seed.Title,
            seed.Body,
            seed.Channel,
            seed.Category,
            seed.RecipientEmail,
            seed.TemplateKey);

        if (created.IsFailure)
            throw new InvalidOperationException(created.Error.Message);

        var notification = created.Value;
        var sentAt = now.AddHours(-seed.SentHoursAgo);

        if (seed.FailureReason is not null)
        {
            var failed = notification.MarkFailed(seed.FailureReason, sentAt);
            if (failed.IsFailure)
                throw new InvalidOperationException(failed.Error.Message);

            return notification;
        }

        var sent = notification.MarkSent(sentAt);
        if (sent.IsFailure)
            throw new InvalidOperationException(sent.Error.Message);

        if (seed.ReadHoursAgo is { } readHours)
        {
            var read = notification.MarkRead(now.AddHours(-readHours));
            if (read.IsFailure)
                throw new InvalidOperationException(read.Error.Message);
        }

        return notification;
    }

    private static void AddPreference(
        NotificationsDbContext db,
        Guid memberId,
        string category,
        bool email,
        bool push,
        bool inApp)
    {
        var preference = NotificationPreference.For(memberId, category);
        preference.Update(email, push, inApp);
        db.NotificationPreferences.Add(preference);
    }

    private static void AddSuppression(NotificationsDbContext db, string email, string reason, string detail)
    {
        var created = EmailSuppression.Create(email, reason, detail);
        if (created.IsFailure)
            throw new InvalidOperationException(created.Error.Message);

        db.EmailSuppressions.Add(created.Value);
    }

    private sealed record TemplateSeed(
        string Key,
        string Name,
        string Category,
        NotificationChannel Channel,
        string TitleTemplate,
        string BodyTemplate);

    private sealed record NotificationSeed(
        Guid Id,
        Guid MemberId,
        string Title,
        string Body,
        NotificationChannel Channel,
        string Category,
        string? RecipientEmail,
        string? TemplateKey,
        int SentHoursAgo,
        int? ReadHoursAgo,
        int AgeHours,
        string? FailureReason = null);
}
