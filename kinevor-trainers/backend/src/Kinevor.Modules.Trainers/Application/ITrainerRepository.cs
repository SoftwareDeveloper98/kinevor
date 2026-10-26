using Kinevor.Modules.Trainers.Domain;
using Kinevor.Platform.Application.Common;

namespace Kinevor.Modules.Trainers.Application;

/// <summary>Queries and writes over trainer aggregates.</summary>
public interface ITrainerRepository : IRepository<Trainer>
{
    /// <summary>Loads every active trainer.</summary>
    Task<IReadOnlyList<Trainer>> GetActiveAsync(CancellationToken ct = default);

    /// <summary>Paged trainer search by name or e-mail.</summary>
    Task<(IReadOnlyList<Trainer> Items, int TotalCount)> SearchAsync(
        string? search, int page, int pageSize, bool onlyActive, CancellationToken ct = default);
}
