namespace Kinevor.Platform.Domain.Common;

/// <summary>
/// Base class for immutable value objects: equality is structural, defined by the components
/// returned from <see cref="GetEqualityComponents"/>.
/// </summary>
public abstract class ValueObject
{
    /// <summary>Components that participate in equality; compared in order.</summary>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>True when both instances have the same type and equal components.</summary>
    public override bool Equals(object? obj) =>
        obj?.GetType() == GetType() &&
        GetEqualityComponents().SequenceEqual(((ValueObject)obj).GetEqualityComponents());

    /// <summary>Hash code derived from the equality components.</summary>
    public override int GetHashCode() =>
        GetEqualityComponents().Aggregate(0, HashCode.Combine);

    /// <summary>Value equality operator.</summary>
    public static bool operator ==(ValueObject? a, ValueObject? b) =>
        a is null ? b is null : a.Equals(b);

    /// <summary>Inequality operator, the negation of <see cref="operator ==(ValueObject, ValueObject)"/>.</summary>
    public static bool operator !=(ValueObject? a, ValueObject? b) => !(a == b);
}
