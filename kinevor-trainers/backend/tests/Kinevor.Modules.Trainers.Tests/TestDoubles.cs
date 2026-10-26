using Kinevor.Modules.Trainers.Application;
using Kinevor.Modules.Trainers.Domain;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Domain.Specifications;

namespace Kinevor.Modules.Trainers.Tests;

internal sealed class FakeTrainerRepository : ITrainerRepository
{
    public List<Trainer> Items { get; } = [];

    public Task<Trainer?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        Task.FromResult(Items.FirstOrDefault(trainer => trainer.Id == id));

    public Task<IReadOnlyList<Trainer>> ListAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Trainer>>(Items);

    public Task<IReadOnlyList<Trainer>> ListAsync(ISpecification<Trainer> spec, CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Trainer>>(Items.Where(spec.Criteria.Compile()).ToList());

    public Task<int> CountAsync(ISpecification<Trainer> spec, CancellationToken ct = default) =>
        Task.FromResult(Items.Count(spec.Criteria.Compile()));

    public Task<bool> AnyAsync(ISpecification<Trainer> spec, CancellationToken ct = default) =>
        Task.FromResult(Items.Any(spec.Criteria.Compile()));

    public void Add(Trainer entity) => Items.Add(entity);

    public void Update(Trainer entity)
    {
        var index = Items.FindIndex(trainer => trainer.Id == entity.Id);
        if (index >= 0) Items[index] = entity;
    }

    public void Remove(Trainer entity) => Items.RemoveAll(trainer => trainer.Id == entity.Id);

    public Task<IReadOnlyList<Trainer>> GetActiveAsync(CancellationToken ct = default) =>
        Task.FromResult<IReadOnlyList<Trainer>>(Items.Where(t => t.Status == Kinevor.Platform.Domain.Enums.TrainerStatus.Active).ToList());

    public Task<(IReadOnlyList<Trainer> Items, int TotalCount)> SearchAsync(
        string? search, int page, int pageSize, bool onlyActive, CancellationToken ct = default)
    {
        var query = Items.AsEnumerable();
        if (onlyActive) query = query.Where(t => t.Status == Kinevor.Platform.Domain.Enums.TrainerStatus.Active);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(t => t.FullName.Contains(term, StringComparison.OrdinalIgnoreCase)
                || t.Email.Value.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var materialized = query.ToList();
        return Task.FromResult<(IReadOnlyList<Trainer>, int)>(
            (materialized.Skip((page - 1) * pageSize).Take(pageSize).ToList(), materialized.Count));
    }
}

internal sealed class FakeUnitOfWork : IUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        SaveCount++;
        return Task.FromResult(1);
    }

    public Task<bool> TrySaveChangesAsync(CancellationToken ct = default)
    {
        SaveCount++;
        return Task.FromResult(true);
    }
}

internal sealed class FakeBranchContext(Guid? branchId) : IBranchContext
{
    public Guid? BranchId { get; } = branchId;
}

internal sealed class FakeCurrentUser : ICurrentUser
{
    public Guid? UserId { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = [];
    public string? Email { get; init; }
    public Guid? BranchId { get; init; }
    public Guid? MemberId { get; init; }
    public Guid? TrainerId { get; init; }
}
