using Kinevor.Modules.Notifications.Domain;
using Xunit;

namespace Kinevor.Modules.Notifications.Tests;

/// <summary>Domain invariants for notifications, preferences, devices, templates and suppressions.</summary>
public class NotificationsDomainTests
{
    private static readonly DateTime Now = new(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc);

    /// <summary>Creating a notification normalizes its fields and starts queued.</summary>
    [Fact]
    public void Create_NormalizesFields()
    {
        var created = Notification.Create(
            Guid.NewGuid(),
            "  Class reminder  ",
            "  Your class starts soon.  ",
            NotificationChannel.InApp,
            "CLASS_REMINDERS",
            "  MAYA@Example.COM ");

        Assert.True(created.IsSuccess);
        Assert.Equal("Class reminder", created.Value.Title);
        Assert.Equal("Your class starts soon.", created.Value.Body);
        Assert.Equal(NotificationCategory.ClassReminders, created.Value.Category);
        Assert.Equal("maya@example.com", created.Value.RecipientEmail);
        Assert.Equal(NotificationStatus.Queued, created.Value.Status);
        Assert.True(created.Value.IsUnread);
        Assert.True(created.Value.IsDue(Now));
    }

    /// <summary>Unknown categories and empty bodies are rejected.</summary>
    [Fact]
    public void Create_RejectsInvalidInput()
    {
        var unknownCategory = Notification.Create(
            Guid.NewGuid(), "Title", "Body", NotificationChannel.InApp, "not_a_category");

        var emptyBody = Notification.Create(
            Guid.NewGuid(), "Title", "  ", NotificationChannel.InApp, NotificationCategory.Offers);

        Assert.True(unknownCategory.IsFailure);
        Assert.Equal("Validation.Category", unknownCategory.Error.Code);
        Assert.True(emptyBody.IsFailure);
        Assert.Equal("Validation.Body", emptyBody.Error.Code);
    }

    /// <summary>The lifecycle only allows queued to sent or failed, then sent to read.</summary>
    [Fact]
    public void Lifecycle_EnforcesTransitions()
    {
        var notification = Notification.Create(
            Guid.NewGuid(), "Title", "Body", NotificationChannel.InApp, NotificationCategory.Offers).Value;

        Assert.True(notification.MarkRead(Now).IsFailure);

        Assert.True(notification.MarkSent(Now).IsSuccess);
        Assert.Equal(NotificationStatus.Sent, notification.Status);
        Assert.Equal(Now, notification.SentAt);
        Assert.True(notification.IsUnread);

        Assert.True(notification.MarkRead(Now.AddMinutes(1)).IsSuccess);
        Assert.Equal(NotificationStatus.Read, notification.Status);
        Assert.Equal(Now.AddMinutes(1), notification.ReadAt);
        Assert.False(notification.IsUnread);

        Assert.True(notification.MarkRead(Now.AddMinutes(2)).IsSuccess);
        Assert.Equal(Now.AddMinutes(1), notification.ReadAt);
    }

    /// <summary>A queued notification can fail once and records the reason.</summary>
    [Fact]
    public void Lifecycle_MarksFailedWithReason()
    {
        var notification = Notification.Create(
            Guid.NewGuid(), "Title", "Body", NotificationChannel.Email, NotificationCategory.Receipts).Value;

        var failed = notification.MarkFailed("Mailbox unavailable.", Now);

        Assert.True(failed.IsSuccess);
        Assert.Equal(NotificationStatus.Failed, notification.Status);
        Assert.Equal("Mailbox unavailable.", notification.FailureReason);
        Assert.False(notification.IsUnread);
        Assert.True(notification.MarkSent(Now).IsFailure);
    }

    /// <summary>Preferences default to enabled and respect the requested channel.</summary>
    [Fact]
    public void Preference_DefaultsAndAllowsChannels()
    {
        var preference = NotificationPreference.For(Guid.NewGuid(), "CLASS_REMINDERS");

        Assert.Equal(NotificationCategory.ClassReminders, preference.Category);
        Assert.True(preference.Allows(NotificationChannel.Email));
        Assert.True(preference.Allows(NotificationChannel.Push));
        Assert.True(preference.Allows(NotificationChannel.InApp));

        preference.Update(email: false, push: true, inApp: false);

        Assert.False(preference.Allows(NotificationChannel.Email));
        Assert.True(preference.Allows(NotificationChannel.Push));
        Assert.False(preference.Allows(NotificationChannel.InApp));
    }

    /// <summary>Device registration normalizes platforms and rejects unknown ones.</summary>
    [Fact]
    public void Device_NormalizesPlatformAndRejectsUnknown()
    {
        var created = DeviceToken.Register(Guid.NewGuid(), " token-123 ", "IOS");

        Assert.True(created.IsSuccess);
        Assert.Equal("token-123", created.Value.Token);
        Assert.Equal("ios", created.Value.Platform);

        var invalid = DeviceToken.Register(Guid.NewGuid(), "token-123", "desktop");
        Assert.True(invalid.IsFailure);
        Assert.Equal("Validation.Platform", invalid.Error.Code);
    }

    /// <summary>Suppressions normalize the address and only accept known reasons.</summary>
    [Fact]
    public void Suppression_NormalizesAndValidatesReason()
    {
        var created = EmailSuppression.Create(" Bounce@Example.com ", "BOUNCE", " mailbox full ");

        Assert.True(created.IsSuccess);
        Assert.Equal("bounce@example.com", created.Value.Email);
        Assert.Equal("bounce", created.Value.Reason);
        Assert.Equal("mailbox full", created.Value.Detail);

        Assert.True(EmailSuppression.Create("a@example.com", "unknown").IsFailure);
        Assert.True(EmailSuppression.Create(" ", "bounce").IsFailure);
    }

    /// <summary>Templates render placeholder values and default to their category.</summary>
    [Fact]
    public void Template_RendersPlaceholders()
    {
        var created = NotificationTemplate.Create(
            "Offer_Title",
            "Offer",
            NotificationCategory.Offers,
            NotificationChannel.InApp,
            "{OfferTitle}",
            "Hi {FirstName}, {OfferBody}");

        Assert.True(created.IsSuccess);
        Assert.Equal("offer_title", created.Value.Key);
        Assert.Equal(NotificationCategory.Offers, created.Value.Category);
        Assert.Equal(NotificationChannel.InApp, created.Value.DefaultChannel);

        var (title, body) = created.Value.Render(new Dictionary<string, string>
        {
            ["OfferTitle"] = "Spring offer",
            ["FirstName"] = "Maya",
            ["OfferBody"] = "Save 10%.",
        });

        Assert.Equal("Spring offer", title);
        Assert.Equal("Hi Maya, Save 10%.", body);
    }

    /// <summary>Templates reject unknown categories.</summary>
    [Fact]
    public void Template_RejectsUnknownCategory()
    {
        var created = NotificationTemplate.Create(
            "key", "Name", "unknown", NotificationChannel.InApp, "Title", "Body");

        Assert.True(created.IsFailure);
        Assert.Equal("Validation.Category", created.Error.Code);
    }

    /// <summary>Categories are normalized and validated.</summary>
    [Fact]
    public void Category_NormalizesAndValidates()
    {
        Assert.Equal("offers", NotificationCategory.Normalize(" OFFERS "));
        Assert.True(NotificationCategory.IsKnown("Offers"));
        Assert.False(NotificationCategory.IsKnown("unknown"));
    }
}
