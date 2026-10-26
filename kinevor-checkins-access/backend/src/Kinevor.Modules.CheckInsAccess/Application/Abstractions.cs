using Kinevor.Modules.CheckInsAccess.Domain;

namespace Kinevor.Modules.CheckInsAccess.Application;

/// <summary>Persistence contract for access member projections.</summary>
public interface IAccessMemberRepository
{
    /// <summary>Finds a projected member by external member id.</summary>
    Task<AccessMember?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Searches members by name, number or e-mail.</summary>
    Task<(IReadOnlyList<AccessMember> Items, int Total)> SearchAsync(
        string? search, int page, int pageSize, CancellationToken ct = default);
}

/// <summary>Persistence contract for branches.</summary>
public interface IClubBranchRepository
{
    /// <summary>Finds a branch by id.</summary>
    Task<ClubBranch?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Lists every branch ordered by name.</summary>
    Task<IReadOnlyList<ClubBranch>> ListAsync(CancellationToken ct = default);
}

/// <summary>Filter applied to visit history queries.</summary>
/// <param name="Search">Name or member-number search term.</param>
/// <param name="MemberId">Optional member filter.</param>
/// <param name="BranchId">Optional branch filter.</param>
/// <param name="From">Inclusive UTC start.</param>
/// <param name="To">Inclusive UTC end.</param>
/// <param name="Open">When set, filters open or closed visits.</param>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Rows per page.</param>
public sealed record CheckInSearch(
    string? Search,
    Guid? MemberId,
    Guid? BranchId,
    DateTime? From,
    DateTime? To,
    bool? Open,
    int Page,
    int PageSize);

/// <summary>Persistence contract for visits.</summary>
public interface ICheckInRepository
{
    /// <summary>Finds a visit by id.</summary>
    Task<CheckIn?> GetByIdAsync(Guid id, CancellationToken ct = default);

    /// <summary>Finds the member's currently open visit, when any.</summary>
    Task<CheckIn?> GetOpenByMemberAsync(Guid memberId, CancellationToken ct = default);

    /// <summary>Finds open visits for several members keyed by member id.</summary>
    Task<IReadOnlyDictionary<Guid, CheckIn>> GetOpenByMembersAsync(
        IReadOnlyCollection<Guid> memberIds, CancellationToken ct = default);

    /// <summary>Lists currently open visits at a branch.</summary>
    Task<IReadOnlyList<CheckIn>> GetOpenByBranchAsync(Guid branchId, CancellationToken ct = default);

    /// <summary>Searches visit history with filters and paging.</summary>
    Task<(IReadOnlyList<CheckIn> Items, int Total)> SearchAsync(CheckInSearch search, CancellationToken ct = default);

    /// <summary>Loads visits starting in a UTC range.</summary>
    Task<IReadOnlyList<CheckIn>> GetRangeAsync(
        DateTime from, DateTime to, Guid? branchId, CancellationToken ct = default);

    /// <summary>Counts open visits at a branch.</summary>
    Task<int> CountOpenAsync(Guid branchId, CancellationToken ct = default);

    /// <summary>Counts visits that started in a UTC range.</summary>
    Task<int> CountVisitsAsync(
        Guid memberId, DateTime from, DateTime to, CancellationToken ct = default);

    /// <summary>Tracks a new visit.</summary>
    void Add(CheckIn checkIn);

    /// <summary>Marks a visit as modified.</summary>
    void Update(CheckIn checkIn);
}

/// <summary>Persistence contract for access audits.</summary>
public interface IAccessAuditRepository
{
    /// <summary>Searches audit records by member, newest first.</summary>
    Task<(IReadOnlyList<AccessAudit> Items, int Total)> SearchAsync(
        Guid? memberId, int page, int pageSize, CancellationToken ct = default);

    /// <summary>Tracks a new audit record.</summary>
    void Add(AccessAudit audit);
}

/// <summary>Unit of work owned by the check-ins and access module.</summary>
public interface ICheckInsAccessUnitOfWork
{
    /// <summary>Persists pending changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
