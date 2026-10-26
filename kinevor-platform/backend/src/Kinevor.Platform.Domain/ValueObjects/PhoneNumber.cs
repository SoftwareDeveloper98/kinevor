using System.Text.RegularExpressions;
using Kinevor.Platform.Domain.Common;

namespace Kinevor.Platform.Domain.ValueObjects;

/// <summary>E.164-compatible phone number with separators stripped.</summary>
public sealed class PhoneNumber : ValueObject
{
    /// <summary>Normalised phone number.</summary>
    public string Value { get; }

    private PhoneNumber(string value) => Value = value;

    /// <summary>Validates and normalises a phone number.</summary>
    public static Result<PhoneNumber> Create(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure<PhoneNumber>(Error.Validation("PhoneNumber", "Phone number is required."));
        var clean = Regex.Replace(value.Trim(), @"[\s\-\(\)]", "");
        if (!Regex.IsMatch(clean, @"^\+?[1-9]\d{6,14}$"))
            return Result.Failure<PhoneNumber>(Error.Validation("PhoneNumber", "Invalid phone number."));
        return Result.Success(new PhoneNumber(clean));
    }

    /// <summary>Components that participate in equality.</summary>
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Value; }

    /// <summary>The number as a string.</summary>
    public override string ToString() => Value;
}
