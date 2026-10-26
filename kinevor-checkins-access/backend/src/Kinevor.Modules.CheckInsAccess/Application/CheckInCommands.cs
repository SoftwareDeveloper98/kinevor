using Kinevor.Modules.CheckInsAccess.Domain;
using MediatR;

namespace Kinevor.Modules.CheckInsAccess.Application;

/// <summary>Records a front-desk check-in.</summary>
/// <param name="MemberId">External member id.</param>
/// <param name="BranchId">Requested branch; defaults to the member's home branch.</param>
/// <param name="Notes">Optional notes.</param>
/// <param name="ActorId">Staff actor identity.</param>
/// <param name="ActorRole">Staff actor role.</param>
public sealed record CheckInCommand(
    Guid MemberId,
    Guid? BranchId,
    string? Notes,
    string ActorId,
    string ActorRole) : IRequest<CheckInDto>;

/// <summary>Handles front-desk check-ins.</summary>
public sealed class CheckInCommandHandler(CheckInWorkflow workflow)
    : IRequestHandler<CheckInCommand, CheckInDto>
{
    /// <summary>Checks the member in at the front desk.</summary>
    public Task<CheckInDto> Handle(CheckInCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        return workflow.CheckInAsync(
            command.MemberId,
            command.BranchId,
            CheckInSource.FrontDesk,
            command.Notes,
            command.ActorId,
            command.ActorRole,
            ct);
    }
}

/// <summary>Closes a visit from the front desk.</summary>
/// <param name="CheckInId">Visit identifier.</param>
/// <param name="ActorId">Staff actor identity.</param>
/// <param name="ActorRole">Staff actor role.</param>
public sealed record CheckOutCommand(Guid CheckInId, string ActorId, string ActorRole)
    : IRequest<CheckInDto>;

/// <summary>Handles front-desk check-outs.</summary>
public sealed class CheckOutCommandHandler(CheckInWorkflow workflow)
    : IRequestHandler<CheckOutCommand, CheckInDto>
{
    /// <summary>Closes the visit.</summary>
    public Task<CheckInDto> Handle(CheckOutCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return workflow.CheckOutAsync(command.CheckInId, command.ActorId, command.ActorRole, ct);
    }
}

/// <summary>Starts a visit from the member application.</summary>
/// <param name="MemberId">Linked external member id.</param>
/// <param name="Notes">Optional notes.</param>
public sealed record SelfCheckInCommand(Guid MemberId, string? Notes = null)
    : IRequest<CheckInDto>;

/// <summary>Handles member self check-ins at the home branch.</summary>
public sealed class SelfCheckInCommandHandler(CheckInWorkflow workflow)
    : IRequestHandler<SelfCheckInCommand, CheckInDto>
{
    /// <summary>Checks the member in at their home branch.</summary>
    public Task<CheckInDto> Handle(SelfCheckInCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);

        return workflow.CheckInAsync(
            command.MemberId,
            null,
            CheckInSource.MemberApp,
            command.Notes,
            command.MemberId.ToString(),
            "Member",
            ct);
    }
}

/// <summary>Closes the member's open visit.</summary>
/// <param name="MemberId">Linked external member id.</param>
public sealed record SelfCheckOutCommand(Guid MemberId) : IRequest<CheckInDto>;

/// <summary>Handles member self check-outs.</summary>
public sealed class SelfCheckOutCommandHandler(CheckInWorkflow workflow)
    : IRequestHandler<SelfCheckOutCommand, CheckInDto>
{
    /// <summary>Closes the member's current visit.</summary>
    public Task<CheckInDto> Handle(SelfCheckOutCommand command, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(command);
        return workflow.CheckOutOpenVisitAsync(command.MemberId, ct);
    }
}
