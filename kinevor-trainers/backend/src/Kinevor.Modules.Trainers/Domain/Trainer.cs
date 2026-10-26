using Kinevor.Platform.Domain.Common;
using Kinevor.Platform.Domain.Enums;
using Kinevor.Platform.Domain.ValueObjects;

namespace Kinevor.Modules.Trainers.Domain;

/// <summary>Trainer profile with specializations, certifications and an aggregate rating.</summary>
public sealed class Trainer : AggregateRoot
{
    /// <summary>First name.</summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>Last name.</summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>First and last name combined.</summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>Primary e-mail address.</summary>
    public Email Email { get; private set; } = null!;

    /// <summary>Primary phone number.</summary>
    public PhoneNumber Phone { get; private set; } = null!;

    /// <summary>Short biography shown to members.</summary>
    public string Bio { get; private set; } = string.Empty;

    /// <summary>Optional profile photo URL.</summary>
    public string? PhotoUrl { get; private set; }

    /// <summary>Employment status.</summary>
    public TrainerStatus Status { get; private set; } = TrainerStatus.Active;

    /// <summary>Class types the trainer can deliver.</summary>
    public List<ClassType> Specializations { get; private set; } = [];

    /// <summary>Certification labels.</summary>
    public List<string> Certifications { get; private set; } = [];

    /// <summary>Years of professional experience.</summary>
    public int YearsExperience { get; private set; }

    /// <summary>Average member rating.</summary>
    public double Rating { get; private set; } = 5.0;

    /// <summary>Number of ratings included in the average.</summary>
    public int TotalRatings { get; private set; }

    /// <summary>Branch the trainer belongs to; <see langword="null"/> for platform-wide trainers.</summary>
    public Guid? BranchId { get; private set; }

    private Trainer() { }

    /// <summary>Assigns the trainer to a branch.</summary>
    public void AssignBranch(Guid branchId) => BranchId = branchId;

    /// <summary>Validates and creates a trainer profile.</summary>
    /// <param name="firstName">First name.</param>
    /// <param name="lastName">Last name.</param>
    /// <param name="email">E-mail address.</param>
    /// <param name="phone">Phone number.</param>
    /// <param name="bio">Short biography.</param>
    /// <param name="specializations">Class types the trainer can deliver.</param>
    /// <param name="certifications">Certification labels.</param>
    /// <param name="yearsExperience">Years of professional experience.</param>
    /// <param name="photoUrl">Optional profile photo URL.</param>
    /// <returns>The created trainer, or a validation error.</returns>
    public static Result<Trainer> Create(
        string firstName, string lastName, string email, string phone,
        string bio, List<ClassType> specializations, List<string> certifications,
        int yearsExperience, string? photoUrl = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return Result.Failure<Trainer>(Error.Validation("FirstName", "First name is required."));
        if (string.IsNullOrWhiteSpace(lastName))
            return Result.Failure<Trainer>(Error.Validation("LastName", "Last name is required."));
        if (yearsExperience is < 0 or > 70)
            return Result.Failure<Trainer>(Error.Validation("YearsExperience", "Years of experience must be between 0 and 70."));

        var emailResult = Email.Create(email);
        if (emailResult.IsFailure) return Result.Failure<Trainer>(emailResult.Error);
        var phoneResult = PhoneNumber.Create(phone);
        if (phoneResult.IsFailure) return Result.Failure<Trainer>(phoneResult.Error);

        return Result.Success(new Trainer
        {
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = emailResult.Value,
            Phone = phoneResult.Value,
            Bio = (bio ?? string.Empty).Trim(),
            Specializations = specializations.ToList(),
            Certifications = certifications.ToList(),
            YearsExperience = yearsExperience,
            PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim(),
        });
    }

    /// <summary>Updates the employment status.</summary>
    public void SetStatus(TrainerStatus status) => Status = status;

    /// <summary>Folds a new rating into the running average.</summary>
    public void AddRating(double rating)
    {
        if (rating is < 0 or > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 0 and 5.");

        Rating = ((Rating * TotalRatings) + rating) / (TotalRatings + 1);
        TotalRatings++;
    }
}
