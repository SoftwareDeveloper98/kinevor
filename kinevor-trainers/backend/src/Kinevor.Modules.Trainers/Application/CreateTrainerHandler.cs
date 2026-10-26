using Kinevor.Modules.Trainers.Domain;
using Kinevor.Platform.Application.Common;
using Kinevor.Platform.Application.Exceptions;
using Kinevor.Platform.Domain.Enums;
using MediatR;

namespace Kinevor.Modules.Trainers.Application;

/// <summary>Handles <see cref="CreateTrainerCommand"/>: parses specializations and creates the trainer.</summary>
public class CreateTrainerHandler(
    ITrainerRepository repo, IUnitOfWork uow, IBranchContext branchContext)
    : IRequestHandler<CreateTrainerCommand, Guid>
{
    /// <summary>Creates the trainer and returns its id.</summary>
    public async Task<Guid> Handle(CreateTrainerCommand cmd, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(cmd);

        var specializations = new List<ClassType>();
        foreach (var value in cmd.Specializations ?? [])
        {
            if (!Enum.TryParse<ClassType>(value, ignoreCase: true, out var parsed))
                throw new DomainException($"Unknown specialization '{value}'.");

            specializations.Add(parsed);
        }

        var result = Trainer.Create(cmd.FirstName, cmd.LastName, cmd.Email, cmd.Phone,
            cmd.Bio, specializations, cmd.Certifications ?? [], cmd.YearsExperience, cmd.PhotoUrl);
        if (result.IsFailure) throw new DomainException(result.Error.Message);

        if (branchContext.BranchId is { } branchId) result.Value.AssignBranch(branchId);

        repo.Add(result.Value);
        await uow.SaveChangesAsync(ct);
        return result.Value.Id;
    }
}
