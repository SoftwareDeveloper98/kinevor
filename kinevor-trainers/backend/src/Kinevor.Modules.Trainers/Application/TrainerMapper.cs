using Kinevor.Modules.Trainers.Domain;

namespace Kinevor.Modules.Trainers.Application;

/// <summary>Maps trainer aggregates to API projections.</summary>
internal static class TrainerMapper
{
    /// <summary>Projects a trainer to its list/detail representation.</summary>
    internal static TrainerSummaryDto ToSummary(Trainer trainer) =>
        new(trainer.Id, trainer.FullName, trainer.Email.Value, trainer.Phone.Value, trainer.Bio,
            trainer.Specializations.Select(s => s.ToString()).ToList(),
            trainer.Rating, trainer.TotalRatings, trainer.YearsExperience, trainer.Status.ToString(),
            trainer.PhotoUrl, trainer.Certifications.ToList());
}
