using Kinevor.Platform.Application.Exceptions;
using Kinevor.Platform.Domain.Common;

namespace Kinevor.Modules.PublicSite.Domain;

/// <summary>Published membership offer shown on the plans page.</summary>
public sealed class MembershipPlan : AggregateRoot
{
    private MembershipPlan()
    {
    }

    /// <summary>Plan name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Stable plan identifier used by the web client.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Short plan summary.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Plan tier label.</summary>
    public string Tier { get; private set; } = string.Empty;

    /// <summary>Price for the billing period.</summary>
    public decimal Price { get; private set; }

    /// <summary>ISO 4217 currency code.</summary>
    public string Currency { get; private set; } = "USD";

    /// <summary>Billing period in months.</summary>
    public int BillingPeriodMonths { get; private set; }

    /// <summary>Benefits included with the plan.</summary>
    public IReadOnlyList<string> Features { get; private set; } = [];

    /// <summary>Optional promotional label.</summary>
    public string? Badge { get; private set; }

    /// <summary>Whether the plan is highlighted in the pricing grid.</summary>
    public bool IsFeatured { get; private set; }

    /// <summary>Whether the plan is visible publicly.</summary>
    public bool IsPublished { get; private set; } = true;

    /// <summary>Display order within the plan grid.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Creates a validated membership plan.</summary>
    public static MembershipPlan Seed(
        Guid id,
        string name,
        string slug,
        string description,
        string tier,
        decimal price,
        string currency,
        int billingPeriodMonths,
        IReadOnlyList<string> features,
        int sortOrder,
        bool isFeatured = false,
        string? badge = null,
        bool isPublished = true)
    {
        if (price < 0)
            throw new DomainException("price cannot be negative.");
        if (billingPeriodMonths <= 0)
            throw new DomainException("billingPeriodMonths must be greater than zero.");

        var normalizedCurrency = Text(currency, nameof(currency), 3).ToUpperInvariant();
        if (normalizedCurrency.Length != 3)
            throw new DomainException("currency must be a three-letter ISO code.");

        return new MembershipPlan
        {
            Id = id,
            Name = Text(name, nameof(name), 160),
            Slug = Text(slug, nameof(slug), 120).ToLowerInvariant(),
            Description = Text(description, nameof(description), 600),
            Tier = Text(tier, nameof(tier), 60),
            Price = decimal.Round(price, 2),
            Currency = normalizedCurrency,
            BillingPeriodMonths = billingPeriodMonths,
            Features = features.Select(feature => Text(feature, nameof(features), 160)).ToArray(),
            Badge = string.IsNullOrWhiteSpace(badge) ? null : badge.Trim(),
            IsFeatured = isFeatured,
            IsPublished = isPublished,
            SortOrder = sortOrder,
        };
    }

    private static string Text(string value, string field, int maxLength)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
            throw new DomainException($"{field} is required.");
        if (normalized.Length > maxLength)
            throw new DomainException($"{field} cannot exceed {maxLength} characters.");
        return normalized;
    }
}

/// <summary>Coach profile surfaced on the trainers page.</summary>
public sealed class Trainer : AggregateRoot
{
    private Trainer()
    {
    }

    /// <summary>First name.</summary>
    public string FirstName { get; private set; } = string.Empty;

    /// <summary>Last name.</summary>
    public string LastName { get; private set; } = string.Empty;

    /// <summary>URL-safe trainer identifier.</summary>
    public string Slug { get; private set; } = string.Empty;

    /// <summary>Coaching role and focus.</summary>
    public string RoleTitle { get; private set; } = string.Empty;

    /// <summary>Short biography.</summary>
    public string Bio { get; private set; } = string.Empty;

    /// <summary>Training specialities.</summary>
    public IReadOnlyList<string> Specializations { get; private set; } = [];

    /// <summary>Professional certifications.</summary>
    public IReadOnlyList<string> Certifications { get; private set; } = [];

    /// <summary>Years of coaching experience.</summary>
    public int YearsExperience { get; private set; }

    /// <summary>Optional profile photo URL.</summary>
    public string? PhotoUrl { get; private set; }

    /// <summary>Home branch identifier.</summary>
    public Guid? BranchId { get; private set; }

    /// <summary>Home branch display name.</summary>
    public string? BranchName { get; private set; }

    /// <summary>Whether the profile is visible publicly.</summary>
    public bool IsPublished { get; private set; } = true;

    /// <summary>Display order within the trainer grid.</summary>
    public int SortOrder { get; private set; }

    /// <summary>Full display name.</summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>Creates a validated trainer profile.</summary>
    public static Trainer Seed(
        Guid id,
        string firstName,
        string lastName,
        string slug,
        string roleTitle,
        string bio,
        IReadOnlyList<string> specializations,
        IReadOnlyList<string> certifications,
        int yearsExperience,
        Guid? branchId,
        string? branchName,
        int sortOrder,
        bool isPublished = true,
        string? photoUrl = null)
    {
        if (yearsExperience < 0)
            throw new DomainException("yearsExperience cannot be negative.");

        return new Trainer
        {
            Id = id,
            FirstName = Text(firstName, nameof(firstName), 80),
            LastName = Text(lastName, nameof(lastName), 80),
            Slug = Text(slug, nameof(slug), 140).ToLowerInvariant(),
            RoleTitle = Text(roleTitle, nameof(roleTitle), 160),
            Bio = Text(bio, nameof(bio), 1200),
            Specializations = specializations.Select(item => Text(item, nameof(specializations), 80)).ToArray(),
            Certifications = certifications.Select(item => Text(item, nameof(certifications), 100)).ToArray(),
            YearsExperience = yearsExperience,
            PhotoUrl = string.IsNullOrWhiteSpace(photoUrl) ? null : photoUrl.Trim(),
            BranchId = branchId,
            BranchName = string.IsNullOrWhiteSpace(branchName) ? null : branchName.Trim(),
            IsPublished = isPublished,
            SortOrder = sortOrder,
        };
    }

    private static string Text(string value, string field, int maxLength)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
            throw new DomainException($"{field} is required.");
        if (normalized.Length > maxLength)
            throw new DomainException($"{field} cannot exceed {maxLength} characters.");
        return normalized;
    }
}

/// <summary>Scheduled group class shown in the weekly public timetable.</summary>
public sealed class ClassSession : AggregateRoot
{
    private ClassSession()
    {
    }

    /// <summary>Class name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Class summary.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Class category used for labels and colour accents.</summary>
    public string Category { get; private set; } = string.Empty;

    /// <summary>Effort label such as Low, Moderate or High.</summary>
    public string Intensity { get; private set; } = "Moderate";

    /// <summary>Coach identifier.</summary>
    public Guid TrainerId { get; private set; }

    /// <summary>Coach display name.</summary>
    public string TrainerName { get; private set; } = string.Empty;

    /// <summary>Start timestamp in gym-local time.</summary>
    public DateTime StartsAt { get; private set; }

    /// <summary>Session length in minutes.</summary>
    public int DurationMinutes { get; private set; }

    /// <summary>Maximum number of participants.</summary>
    public int Capacity { get; private set; }

    /// <summary>Studio or floor name.</summary>
    public string Room { get; private set; } = string.Empty;

    /// <summary>Host branch identifier.</summary>
    public Guid BranchId { get; private set; }

    /// <summary>Host branch display name.</summary>
    public string BranchName { get; private set; } = string.Empty;

    /// <summary>Whether the session is visible publicly.</summary>
    public bool IsPublished { get; private set; } = true;

    /// <summary>Creates a validated class session.</summary>
    public static ClassSession Seed(
        Guid id,
        string name,
        string description,
        string category,
        string intensity,
        Guid trainerId,
        string trainerName,
        DateTime startsAt,
        int durationMinutes,
        int capacity,
        string room,
        Guid branchId,
        string branchName,
        bool isPublished = true)
    {
        if (trainerId == Guid.Empty)
            throw new DomainException("trainerId is required.");
        if (branchId == Guid.Empty)
            throw new DomainException("branchId is required.");
        if (durationMinutes is < 10 or > 240)
            throw new DomainException("durationMinutes must be between 10 and 240.");
        if (capacity <= 0)
            throw new DomainException("capacity must be greater than zero.");

        return new ClassSession
        {
            Id = id,
            Name = Text(name, nameof(name), 160),
            Description = Text(description, nameof(description), 600),
            Category = Text(category, nameof(category), 80),
            Intensity = Text(intensity, nameof(intensity), 40),
            TrainerId = trainerId,
            TrainerName = Text(trainerName, nameof(trainerName), 160),
            StartsAt = startsAt,
            DurationMinutes = durationMinutes,
            Capacity = capacity,
            Room = Text(room, nameof(room), 120),
            BranchId = branchId,
            BranchName = Text(branchName, nameof(branchName), 160),
            IsPublished = isPublished,
        };
    }

    private static string Text(string value, string field, int maxLength)
    {
        var normalized = value.Trim();
        if (normalized.Length == 0)
            throw new DomainException($"{field} is required.");
        if (normalized.Length > maxLength)
            throw new DomainException($"{field} cannot exceed {maxLength} characters.");
        return normalized;
    }
}
