using Kinevor.Modules.Trainers.Domain;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using MediatR;

namespace Kinevor.Modules.Trainers.Application;

/// <summary>Fetches one trainer; trainers may only view their own profile.</summary>
/// <param name="Id">Trainer identifier.</param>
public record GetTrainerByIdQuery(Guid Id) : IRequest<TrainerSummaryDto>;

/// <summary>Handles <see cref="GetTrainerByIdQuery"/>.</summary>
public class GetTrainerByIdHandler(ITrainerRepository repo, ICurrentUser currentUser)
    : IRequestHandler<GetTrainerByIdQuery, TrainerSummaryDto>
{
    /// <summary>Returns the trainer summary.</summary>
    public async Task<TrainerSummaryDto> Handle(GetTrainerByIdQuery q, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(q);

        var trainer = await repo.GetByIdAsync(q.Id, ct)
            ?? throw new NotFoundException(nameof(Trainer), q.Id);

        if (currentUser.IsTrainerOnly() && trainer.Id != currentUser.TrainerId)
            throw new ForbiddenException("Trainers may only view their own profile.");

        return TrainerMapper.ToSummary(trainer);
    }
}
