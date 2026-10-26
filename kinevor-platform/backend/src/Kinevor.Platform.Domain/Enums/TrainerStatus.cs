namespace Kinevor.Platform.Domain.Enums;

/// <summary>Employment state of a trainer.</summary>
public enum TrainerStatus
{
    /// <summary>Onboarded and taking bookings.</summary>
    Active,

    /// <summary>Temporarily away, not taking bookings.</summary>
    OnLeave,

    /// <summary>No longer taking bookings.</summary>
    Inactive,
}
