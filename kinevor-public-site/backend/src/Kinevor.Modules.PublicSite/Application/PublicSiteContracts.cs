namespace Kinevor.Modules.PublicSite.Application;

/// <summary>Aggregated brand, contact and social-proof payload for the public landing page.</summary>
public sealed record PublicSiteDto(
    string Name,
    string Tagline,
    string Description,
    string Story,
    string Eyebrow,
    string Phone,
    string Email,
    string Address,
    string City,
    string InstagramHandle,
    int MemberCount,
    int TrainerCount,
    int WeeklyClasses,
    decimal Rating,
    int FoundedYear,
    IReadOnlyList<string> Highlights,
    IReadOnlyList<BranchDto> Branches);

/// <summary>Public location card.</summary>
public sealed record BranchDto(
    Guid Id,
    string Name,
    string Slug,
    string Neighbourhood,
    string Address,
    string City,
    string Phone,
    string OpeningHours,
    string Parking,
    IReadOnlyList<string> Amenities,
    int Capacity,
    double Latitude,
    double Longitude,
    string Theme);

/// <summary>Published membership plan.</summary>
public sealed record MembershipPlanDto(
    Guid Id,
    string Name,
    string Slug,
    string Description,
    string Tier,
    decimal Price,
    string Currency,
    int BillingPeriodMonths,
    string BillingLabel,
    IReadOnlyList<string> Features,
    string? Badge,
    bool IsFeatured);

/// <summary>Published coach card.</summary>
public sealed record TrainerDto(
    Guid Id,
    string FullName,
    string Slug,
    string RoleTitle,
    string Bio,
    IReadOnlyList<string> Specializations,
    IReadOnlyList<string> Certifications,
    int YearsExperience,
    string? BranchName,
    string? PhotoUrl);

/// <summary>Upcoming class in the public timetable.</summary>
public sealed record ClassSessionDto(
    Guid Id,
    string Name,
    string Description,
    string Category,
    string Intensity,
    Guid TrainerId,
    string TrainerName,
    DateTime StartsAt,
    int DurationMinutes,
    int Capacity,
    string Room,
    Guid BranchId,
    string BranchName);

/// <summary>Published frequently asked question.</summary>
public sealed record FaqDto(Guid Id, string Category, string Question, string Answer);

/// <summary>Blog index card.</summary>
public sealed record BlogPostSummaryDto(
    Guid Id,
    string Slug,
    string Title,
    string Excerpt,
    string Author,
    DateTime PublishedAt,
    string? ImageUrl);

/// <summary>Full blog article.</summary>
public sealed record BlogPostDto(
    Guid Id,
    string Slug,
    string Title,
    string Excerpt,
    string Content,
    string Author,
    DateTime PublishedAt,
    string? ImageUrl);

/// <summary>Trial request body accepted by the public API.</summary>
public sealed record TrialRequest(
    string FirstName,
    string LastName,
    string Email,
    string Phone,
    DateTime? PreferredDate,
    Guid? ClassSessionId,
    Guid? BranchId,
    string? Goal,
    string? Notes,
    bool ConsentToContact);

/// <summary>Website enquiry body accepted by the public API.</summary>
public sealed record LeadRequest(
    string Name,
    string Email,
    string? Phone,
    string Interest,
    string Message,
    bool ConsentToContact);

/// <summary>Identifier returned after a trial request is stored.</summary>
public sealed record TrialCreatedDto(Guid Id, string Message);

/// <summary>Identifier returned after an enquiry is stored.</summary>
public sealed record LeadCreatedDto(Guid Id, string Message);

/// <summary>Normalized lead row combining trial requests and enquiries.</summary>
public sealed record LeadDto(
    Guid Id,
    string Kind,
    string Name,
    string Email,
    string? Phone,
    string? Interest,
    string? Message,
    DateTime? PreferredDate,
    string? ClassName,
    string? BranchName,
    string Status,
    DateTime CreatedAt);

/// <summary>Paged response used by the staff lead inbox.</summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages,
    bool HasNext,
    bool HasPrev);
