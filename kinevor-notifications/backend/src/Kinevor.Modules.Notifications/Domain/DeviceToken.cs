using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.Notifications.Domain;

/// <summary>Push-notification device token registered by a member's app.</summary>
public sealed class DeviceToken : Entity
{
    private DeviceToken()
    {
    }

    /// <summary>Member the token belongs to.</summary>
    public Guid MemberId { get; private set; }

    /// <summary>Opaque device token.</summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>Platform name: ios, android or web.</summary>
    public string Platform { get; private set; } = string.Empty;

    /// <summary>Validates and registers a device token.</summary>
    public static Result<DeviceToken> Register(Guid memberId, string token, string platform)
    {
        if (memberId == Guid.Empty)
            return Result.Failure<DeviceToken>(Error.Validation("MemberId", "Member is required."));

        if (string.IsNullOrWhiteSpace(token))
            return Result.Failure<DeviceToken>(Error.Validation("Token", "Device token is required."));

        if (!TryNormalizePlatform(platform, out var normalizedPlatform))
            return Result.Failure<DeviceToken>(Error.Validation("Platform", "Platform must be ios, android or web."));

        return Result.Success(new DeviceToken
        {
            MemberId = memberId,
            Token = token.Trim(),
            Platform = normalizedPlatform,
        });
    }

    /// <summary>Re-assigns an existing token to a member and platform.</summary>
    public void Assign(Guid memberId, string platform)
    {
        if (TryNormalizePlatform(platform, out var normalizedPlatform))
            Platform = normalizedPlatform;

        MemberId = memberId;
    }

    /// <summary>Whether a platform name is one of the supported values.</summary>
    public static bool TryNormalizePlatform(string? platform, out string normalized)
    {
        normalized = (platform ?? string.Empty).Trim().ToLowerInvariant();
        return normalized is "ios" or "android" or "web";
    }
}
