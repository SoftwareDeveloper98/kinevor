namespace Kinevor.Modules.Notifications.Domain;

/// <summary>Delivery channel of a notification record.</summary>
public enum NotificationChannel
{
    /// <summary>E-mail delivery, subject to address suppression.</summary>
    Email,

    /// <summary>Device push delivery, requiring a registered device token.</summary>
    Push,

    /// <summary>In-app inbox delivery.</summary>
    InApp,
}

/// <summary>Lifecycle status of a notification record.</summary>
public enum NotificationStatus
{
    /// <summary>Accepted and waiting for the delivery simulation.</summary>
    Queued,

    /// <summary>Handed to the delivery simulation successfully.</summary>
    Sent,

    /// <summary>Opened by the recipient in the in-app inbox.</summary>
    Read,

    /// <summary>Delivery was blocked or rejected; see the failure reason.</summary>
    Failed,
}
