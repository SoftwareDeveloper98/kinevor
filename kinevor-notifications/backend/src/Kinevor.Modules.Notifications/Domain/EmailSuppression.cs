using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.Notifications.Domain;

/// <summary>An e-mail address that must not receive further messages, with the reason it was recorded.</summary>
public sealed class EmailSuppression : Entity
{
    /// <summary>Well-known reason values.</summary>
    public static readonly string[] Reasons = ["bounce", "complaint", "unsubscribe"];

    private EmailSuppression()
    {
    }

    /// <summary>Address (lower-cased) that is suppressed.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>Machine-readable reason: <c>bounce</c>, <c>complaint</c> or <c>unsubscribe</c>.</summary>
    public string Reason { get; private set; } = string.Empty;

    /// <summary>Free-text detail supplied by the provider or member, when any.</summary>
    public string? Detail { get; private set; }

    /// <summary>Creates a suppression row.</summary>
    public static Result<EmailSuppression> Create(string email, string reason, string? detail = null)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Failure<EmailSuppression>(Error.Validation("Email", "E-mail address is required."));

        if (!TryNormalizeReason(reason, out var normalizedReason))
            return Result.Failure<EmailSuppression>(Error.Validation("Reason",
                $"Unsupported suppression reason '{reason}'."));

        return Result.Success(new EmailSuppression
        {
            Email = NormalizeEmail(email),
            Reason = normalizedReason,
            Detail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim(),
        });
    }

    /// <summary>Re-records the reason and detail for an existing suppression.</summary>
    public void Update(string reason, string? detail)
    {
        if (TryNormalizeReason(reason, out var normalizedReason))
            Reason = normalizedReason;

        Detail = string.IsNullOrWhiteSpace(detail) ? null : detail.Trim();
    }

    /// <summary>Trims and lower-cases an e-mail address.</summary>
    public static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    /// <summary>Whether a reason value is one of <see cref="Reasons"/>.</summary>
    public static bool TryNormalizeReason(string? reason, out string normalized)
    {
        normalized = (reason ?? string.Empty).Trim().ToLowerInvariant();
        return Reasons.Contains(normalized);
    }
}
