namespace Kinevor.Modules.Trainers.Application;

/// <summary>Trainer projection for lists and detail views.</summary>
public record TrainerSummaryDto(
    Guid Id, string FullName, string Email, string Phone,
    string Bio, List<string> Specializations, double Rating, int TotalRatings,
    int YearsExperience, string Status, string? PhotoUrl,
    List<string> Certifications);
