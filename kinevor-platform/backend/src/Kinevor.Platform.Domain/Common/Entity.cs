namespace Kinevor.Platform.Domain.Common;

/// <summary>
/// Base class for persistence-tracked domain types. Audit timestamps are stamped by the
/// unit of work when the row is written; the domain never reads the ambient clock for them.
/// </summary>
public abstract class Entity
{
    /// <summary>Unique identifier, assigned when the instance is created.</summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>UTC timestamp of the insert that first persisted this row.</summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>UTC timestamp of the most recent update; <see langword="null"/> until first updated.</summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>Soft-delete marker honored by the persistence layer.</summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>Compares identity only: same runtime type and same <see cref="Id"/>.</summary>
    public override bool Equals(object? obj) =>
        obj is Entity other && GetType() == other.GetType() && Id == other.Id;

    /// <summary>Combines the runtime type and <see cref="Id"/> so identity hash codes stay stable.</summary>
    public override int GetHashCode() => HashCode.Combine(GetType(), Id);
}
