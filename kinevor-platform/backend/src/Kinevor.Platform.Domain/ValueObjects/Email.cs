using System.Text.RegularExpressions;
using Kinevor.Platform.Domain.Common;

namespace Kinevor.Platform.Domain.ValueObjects;

/// <summary>Validated e-mail address, stored lower-case and trimmed.</summary>
public sealed class Email : ValueObject
{
    /// <summary>Normalised e-mail address.</summary>
    public string Value { get; }

    private Email(string value) => Value = value;

    /// <summary>Validates and normalises an e-mail address.</summary>
    public static Result<Email> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<Email>(Error.Validation("Email", "Email is required."));
        var trimmed = value.Trim().ToLowerInvariant();
        if (!Regex.IsMatch(trimmed, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            return Result.Failure<Email>(Error.Validation("Email", "Invalid email format."));
        return Result.Success(new Email(trimmed));
    }

    /// <summary>Components that participate in equality.</summary>
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    /// <summary>The address as a string.</summary>
    public override string ToString() => Value;
}
