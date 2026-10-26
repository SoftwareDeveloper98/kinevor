using Kinevor.Modules.CheckInsAccess.Domain;
using Kinevor.Platform.Application.Exceptions;

namespace Kinevor.Modules.CheckInsAccess.Application;

/// <summary>Coordinates eligibility, duplicate detection, visit creation and audit writes.</summary>
public sealed class CheckInWorkflow(
    IAccessMemberRepository members,
    ICheckInRepository checkIns,
    IClubBranchRepository branches,
    IAccessAuditRepository audits,
    ICheckInsAccessUnitOfWork unitOfWork,
    TimeProvider clock)
{
    /// <summary>Checks a member in after re-evaluating access at the branch.</summary>
    /// <param name="memberId">External member id.</param>
    /// <param name="branchId">Requested branch; defaults to the member's home branch.</param>
    /// <param name="source">Where the check-in originated.</param>
    /// <param name="notes">Optional notes.</param>
    /// <param name="actorId">Actor identity.</param>
    /// <param name="actorRole">Actor role.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The created visit.</returns>
    public async Task<CheckInDto> CheckInAsync(
        Guid memberId,
        Guid? branchId,
        CheckInSource source,
        string? notes,
        string actorId,
        string actorRole,
        CancellationToken ct = default)
    {
        var member = await members.GetByIdAsync(memberId, ct)
            ?? throw new NotFoundException("Member", memberId);
        var requestedBranchId = branchId ?? member.HomeBranchId
            ?? throw new DomainException("A branch is required to check in this member.");
        var branch = await branches.GetByIdAsync(requestedBranchId, ct)
            ?? throw new NotFoundException("Branch", requestedBranchId);
        var now = clock.GetUtcNow().UtcDateTime;
        var decision = AccessPolicy.Evaluate(member, now, branch.Id);

        if (!decision.IsAllowed)
        {
            audits.Add(AccessAudit.Record(
                member, branch.Id, AccessAuditAction.AccessDenied, AccessOutcome.Denied,
                decision, source, actorId, actorRole, now));
            await unitOfWork.SaveChangesAsync(ct);
            throw new DomainException(decision.Message);
        }

        var existing = await checkIns.GetOpenByMemberAsync(member.Id, ct);
        if (existing is not null)
        {
            var duplicate = AccessDecision.Deny(
                "Access.AlreadyCheckedIn",
                "This member already has an open visit. Check them out before starting another.");
            audits.Add(AccessAudit.Record(
                member, branch.Id, AccessAuditAction.AccessDenied, AccessOutcome.Denied,
                duplicate, source, actorId, actorRole, now, existing.Id));
            await unitOfWork.SaveChangesAsync(ct);
            throw new ConflictException(duplicate.Message);
        }

        var recorded = CheckIn.Record(
            now, member, branch.Id, branch.Name, source, notes, actorId, actorRole, decision);
        if (recorded.IsFailure)
            throw new DomainException(recorded.Error.Message);

        var visit = recorded.Value;
        checkIns.Add(visit);
        audits.Add(AccessAudit.Record(
            member, branch.Id, AccessAuditAction.CheckIn, AccessOutcome.Allowed,
            decision, source, actorId, actorRole, now, visit.Id));
        await unitOfWork.SaveChangesAsync(ct);
        return CheckInMapper.ToDto(visit, now);
    }

    /// <summary>Closes an open visit and writes an audit record.</summary>
    /// <param name="checkInId">Visit identifier.</param>
    /// <param name="actorId">Actor identity.</param>
    /// <param name="actorRole">Actor role.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The closed visit.</returns>
    public async Task<CheckInDto> CheckOutAsync(
        Guid checkInId, string actorId, string actorRole, CancellationToken ct = default)
    {
        var visit = await checkIns.GetByIdAsync(checkInId, ct)
            ?? throw new NotFoundException("Check-in", checkInId);
        if (visit.CheckedOutAt is not null)
            throw new ConflictException("This visit is already closed.");

        var member = await members.GetByIdAsync(visit.MemberId, ct)
            ?? throw new NotFoundException("Member", visit.MemberId);
        var now = clock.GetUtcNow().UtcDateTime;
        var closed = visit.CheckOut(now);
        if (closed.IsFailure)
            throw new ConflictException(closed.Error.Message);

        checkIns.Update(visit);
        audits.Add(AccessAudit.Record(
            member, visit.BranchId, AccessAuditAction.CheckOut, AccessOutcome.Allowed,
            AccessDecision.Allow(), visit.Source, actorId, actorRole, now, visit.Id));
        await unitOfWork.SaveChangesAsync(ct);
        return CheckInMapper.ToDto(visit, now);
    }

    /// <summary>Closes the member's currently open visit.</summary>
    /// <param name="memberId">External member id.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The closed visit.</returns>
    public async Task<CheckInDto> CheckOutOpenVisitAsync(Guid memberId, CancellationToken ct = default)
    {
        var open = await checkIns.GetOpenByMemberAsync(memberId, ct)
            ?? throw new ConflictException("There is no open visit to check out.");
        return await CheckOutAsync(open.Id, memberId.ToString(), "Member", ct);
    }
}
