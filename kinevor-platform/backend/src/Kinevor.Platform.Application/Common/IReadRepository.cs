using Kinevor.Platform.Domain.Specifications;

namespace Kinevor.Platform.Application.Common;

/// <summary>Read side of the repository contract for an aggregate.</summary>
/// <typeparam name="T">Aggregate type.</typeparam>
public interface IReadRepository<T> where T : class
{
    /// <summary>Loads an aggregate by id, or <see langword="null"/> when missing.</summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Loads every aggregate.</summary>
    Task<IReadOnlyList<T>> ListAsync(CancellationToken ct = default);

    /// <summary>Loads aggregates matching a specification.</summary>
    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> spec, CancellationToken ct = default);

    /// <summary>Counts aggregates matching a specification.</summary>
    Task<int> CountAsync(ISpecification<T> spec, CancellationToken ct = default);

    /// <summary>Whether any aggregate matches a specification.</summary>
    Task<bool> AnyAsync(ISpecification<T> spec, CancellationToken ct = default);
}
