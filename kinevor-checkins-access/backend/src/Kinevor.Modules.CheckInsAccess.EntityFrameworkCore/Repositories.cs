using Kinevor.Modules.CheckInsAccess.Application;
using Kinevor.Modules.CheckInsAccess.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kinevor.Modules.CheckInsAccess.EntityFrameworkCore;

/// <summary>EF Core access member repository.</summary>
public sealed class AccessMemberRepository(CheckInsDbContext db) : IAccessMemberRepository
{
    /// <summary>Finds a projected member by external member id.</summary>
    public Task<AccessMember?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.AccessMembers.FirstOrDefaultAsync(member => member.Id == id, ct);

    /// <summary>Searches members by name, member number or e-mail.</summary>
    public async Task<(IReadOnlyList<AccessMember> Items, int Total)> SearchAsync(
        string? search, int page, int pageSize, CancellationToken ct = default)
    {
        IQueryable<AccessMember> query = db.AccessMembers.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = $"%{search.Trim()}%";
            query = query.Where(member =>
                EF.Functions.Like(member.FullName, term) ||
                EF.Functions.Like(member.MemberNumber, term) ||
                (member.Email != null && EF.Functions.Like(member.Email, term)));
        }

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderBy(member => member.FullName)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }
}

/// <summary>EF Core branch repository.</summary>
public sealed class ClubBranchRepository(CheckInsDbContext db) : IClubBranchRepository
{
    /// <summary>Finds a branch by id.</summary>
    public Task<ClubBranch?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.Branches.FirstOrDefaultAsync(branch => branch.Id == id, ct);

    /// <summary>Lists every branch ordered by name.</summary>
    public async Task<IReadOnlyList<ClubBranch>> ListAsync(CancellationToken ct = default) =>
        await db.Branches.AsNoTracking().OrderBy(branch => branch.Name).ToListAsync(ct);
}

/// <summary>EF Core visit repository.</summary>
public sealed class CheckInRepository(CheckInsDbContext db) : ICheckInRepository
{
    /// <summary>Finds a tracked visit by id.</summary>
    public Task<CheckIn?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        db.CheckIns.FirstOrDefaultAsync(checkIn => checkIn.Id == id, ct);

    /// <summary>Finds the member's open visit.</summary>
    public Task<CheckIn?> GetOpenByMemberAsync(Guid memberId, CancellationToken ct = default) =>
        db.CheckIns.FirstOrDefaultAsync(
            checkIn => checkIn.MemberId == memberId && checkIn.CheckedOutAt == null, ct);

    /// <summary>Finds open visits for several members.</summary>
    public async Task<IReadOnlyDictionary<Guid, CheckIn>> GetOpenByMembersAsync(
        IReadOnlyCollection<Guid> memberIds, CancellationToken ct = default)
    {
        if (memberIds.Count == 0)
            return new Dictionary<Guid, CheckIn>();

        var visits = await db.CheckIns.AsNoTracking()
            .Where(checkIn => checkIn.CheckedOutAt == null && memberIds.Contains(checkIn.MemberId))
            .ToListAsync(ct);
        return visits
            .GroupBy(checkIn => checkIn.MemberId)
            .ToDictionary(group => group.Key, group => group.First());
    }

    /// <summary>Lists open visits at a branch newest first.</summary>
    public async Task<IReadOnlyList<CheckIn>> GetOpenByBranchAsync(
        Guid branchId, CancellationToken ct = default) =>
        await db.CheckIns.AsNoTracking()
            .Where(checkIn => checkIn.BranchId == branchId && checkIn.CheckedOutAt == null)
            .OrderByDescending(checkIn => checkIn.CheckedInAt)
            .ToListAsync(ct);

    /// <summary>Searches visit history with filters and paging.</summary>
    public async Task<(IReadOnlyList<CheckIn> Items, int Total)> SearchAsync(
        CheckInSearch search, CancellationToken ct = default)
    {
        IQueryable<CheckIn> query = db.CheckIns.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search.Search))
        {
            var term = $"%{search.Search.Trim()}%";
            query = query.Where(checkIn =>
                EF.Functions.Like(checkIn.MemberName, term) ||
                EF.Functions.Like(checkIn.MemberNumber, term));
        }

        if (search.MemberId is { } memberId)
            query = query.Where(checkIn => checkIn.MemberId == memberId);
        if (search.BranchId is { } branchId)
            query = query.Where(checkIn => checkIn.BranchId == branchId);
        if (search.From is { } from)
            query = query.Where(checkIn => checkIn.CheckedInAt >= from);
        if (search.To is { } to)
            query = query.Where(checkIn => checkIn.CheckedInAt <= to);
        if (search.Open is { } open)
            query = open
                ? query.Where(checkIn => checkIn.CheckedOutAt == null)
                : query.Where(checkIn => checkIn.CheckedOutAt != null);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(checkIn => checkIn.CheckedInAt)
            .Skip((search.Page - 1) * search.PageSize)
            .Take(search.PageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    /// <summary>Loads visits starting in a UTC range.</summary>
    public async Task<IReadOnlyList<CheckIn>> GetRangeAsync(
        DateTime from, DateTime to, Guid? branchId, CancellationToken ct = default)
    {
        var query = db.CheckIns.AsNoTracking()
            .Where(checkIn => checkIn.CheckedInAt >= from && checkIn.CheckedInAt < to);
        if (branchId is { } branch)
            query = query.Where(checkIn => checkIn.BranchId == branch);
        return await query.ToListAsync(ct);
    }

    /// <summary>Counts open visits at a branch.</summary>
    public Task<int> CountOpenAsync(Guid branchId, CancellationToken ct = default) =>
        db.CheckIns.CountAsync(
            checkIn => checkIn.BranchId == branchId && checkIn.CheckedOutAt == null, ct);

    /// <summary>Counts visits that started in a UTC range.</summary>
    public Task<int> CountVisitsAsync(
        Guid memberId, DateTime from, DateTime to, CancellationToken ct = default) =>
        db.CheckIns.CountAsync(
            checkIn => checkIn.MemberId == memberId &&
                checkIn.CheckedInAt >= from &&
                checkIn.CheckedInAt < to, ct);

    /// <summary>Tracks a new visit.</summary>
    public void Add(CheckIn checkIn) => db.CheckIns.Add(checkIn);

    /// <summary>Marks a visit as modified.</summary>
    public void Update(CheckIn checkIn) => db.CheckIns.Update(checkIn);
}

/// <summary>EF Core access audit repository.</summary>
public sealed class AccessAuditRepository(CheckInsDbContext db) : IAccessAuditRepository
{
    /// <summary>Searches audit records newest first.</summary>
    public async Task<(IReadOnlyList<AccessAudit> Items, int Total)> SearchAsync(
        Guid? memberId, int page, int pageSize, CancellationToken ct = default)
    {
        IQueryable<AccessAudit> query = db.AccessAudits.AsNoTracking();
        if (memberId is { } id)
            query = query.Where(audit => audit.MemberId == id);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(audit => audit.OccurredAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    /// <summary>Tracks a new audit record.</summary>
    public void Add(AccessAudit audit) => db.AccessAudits.Add(audit);
}
