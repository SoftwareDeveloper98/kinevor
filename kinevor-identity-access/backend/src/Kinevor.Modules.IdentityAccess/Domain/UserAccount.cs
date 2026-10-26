using Kinevor.Platform.Domain.Common;
using Kinevor.Platform.Domain.ValueObjects;

namespace Kinevor.Modules.IdentityAccess.Domain;

/// <summary>Account that can authenticate against the Kinevor platform.</summary>
public sealed class UserAccount : AggregateRoot
{
    /// <summary>Login e-mail address.</summary>
    public Email Email { get; private set; } = null!;

    /// <summary>Display name shown in the console.</summary>
    public string DisplayName { get; private set; } = string.Empty;

    /// <summary>Platform role granted to the account.</summary>
    public string Role { get; private set; } = string.Empty;

    /// <summary>Encoded PBKDF2 password hash.</summary>
    public string PasswordHash { get; private set; } = string.Empty;

    /// <summary>Whether the account may sign in.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>Whether a second factor is enrolled.</summary>
    public bool MfaEnabled { get; private set; }

    /// <summary>UTC timestamp of the last successful login.</summary>
    public DateTime? LastLoginAt { get; private set; }

    private UserAccount() { }

    /// <summary>Validates and creates an account.</summary>
    /// <param name="email">Login e-mail address.</param>
    /// <param name="displayName">Display name.</param>
    /// <param name="role">Platform role.</param>
    /// <param name="passwordHash">Already encoded password hash.</param>
    /// <param name="now">Current UTC timestamp.</param>
    /// <returns>The account, or a validation error.</returns>
    public static Result<UserAccount> Create(
        string email, string displayName, string role, string passwordHash, DateTime now)
    {
        var emailResult = Email.Create(email);
        if (emailResult.IsFailure) return Result.Failure<UserAccount>(emailResult.Error);

        if (string.IsNullOrWhiteSpace(displayName))
            return Result.Failure<UserAccount>(Error.Validation("DisplayName", "Display name is required."));
        if (string.IsNullOrWhiteSpace(passwordHash))
            return Result.Failure<UserAccount>(Error.Validation("PasswordHash", "Password hash is required."));
        if (!AllowedRoles.Contains(role))
            return Result.Failure<UserAccount>(Error.Validation("Role", "Role must be Admin, Staff, Trainer or Member."));

        return Result.Success(new UserAccount
        {
            Email = emailResult.Value,
            DisplayName = displayName.Trim(),
            Role = role,
            PasswordHash = passwordHash,
            CreatedAt = now,
        });
    }

    /// <summary>Records a successful login.</summary>
    public void RecordLogin(DateTime now) => LastLoginAt = now;

    /// <summary>Enables or disables the second factor.</summary>
    public void SetMfa(bool enabled) => MfaEnabled = enabled;

    /// <summary>Replaces the encoded password hash (used when rehashing to a higher iteration count).</summary>
    public void UpdatePasswordHash(string passwordHash) => PasswordHash = passwordHash;

    /// <summary>Enables or disables sign-in.</summary>
    public void SetActive(bool active) => IsActive = active;

    private static readonly HashSet<string> AllowedRoles =
        new(StringComparer.Ordinal) { "Admin", "Staff", "Trainer", "Member" };
}
