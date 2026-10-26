namespace Kinevor.Platform.Application.Common;

/// <summary>Persists staged changes for the current module.</summary>
public interface IUnitOfWork
{
    /// <summary>Persists pending changes and returns the number of state entries written.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// Persists pending changes, returning <see langword="false"/> when a concurrent write changed
    /// the same row (optimistic-concurrency conflict) instead of throwing.
    /// </summary>
    Task<bool> TrySaveChangesAsync(CancellationToken ct = default);
}
