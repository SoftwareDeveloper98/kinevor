using Kinevor.Modules.PublicSite.Domain;
using Kinevor.Platform.Application.Exceptions;
using MediatR;

namespace Kinevor.Modules.PublicSite.Application;

/// <summary>Loads the aggregated public landing-page payload.</summary>
public sealed record GetPublicSiteQuery : IRequest<PublicSiteDto>;

/// <summary>Loads every published location.</summary>
public sealed record GetBranchesQuery : IRequest<IReadOnlyList<BranchDto>>;

/// <summary>Loads one published location by slug.</summary>
public sealed record GetBranchQuery(string Slug) : IRequest<BranchDto>;

/// <summary>Loads every published membership plan.</summary>
public sealed record GetPlansQuery : IRequest<IReadOnlyList<MembershipPlanDto>>;

/// <summary>Loads every published coach.</summary>
public sealed record GetTrainersQuery : IRequest<IReadOnlyList<TrainerDto>>;

/// <summary>Loads the public timetable for an inclusive date window.</summary>
public sealed record GetScheduleQuery(DateTime? From, DateTime? To, Guid? BranchId)
    : IRequest<IReadOnlyList<ClassSessionDto>>;

/// <summary>Loads every published FAQ.</summary>
public sealed record GetFaqsQuery : IRequest<IReadOnlyList<FaqDto>>;

/// <summary>Loads the published blog index.</summary>
public sealed record GetBlogPostsQuery : IRequest<IReadOnlyList<BlogPostSummaryDto>>;

/// <summary>Loads one published blog article by slug.</summary>
public sealed record GetBlogPostQuery(string Slug) : IRequest<BlogPostDto>;

internal sealed class PublicSiteQueryHandler(
    IPublicSiteRepository repository,
    TimeProvider timeProvider)
    : IRequestHandler<GetPublicSiteQuery, PublicSiteDto>,
      IRequestHandler<GetBranchesQuery, IReadOnlyList<BranchDto>>,
      IRequestHandler<GetBranchQuery, BranchDto>,
      IRequestHandler<GetPlansQuery, IReadOnlyList<MembershipPlanDto>>,
      IRequestHandler<GetTrainersQuery, IReadOnlyList<TrainerDto>>,
      IRequestHandler<GetScheduleQuery, IReadOnlyList<ClassSessionDto>>,
      IRequestHandler<GetFaqsQuery, IReadOnlyList<FaqDto>>,
      IRequestHandler<GetBlogPostsQuery, IReadOnlyList<BlogPostSummaryDto>>,
      IRequestHandler<GetBlogPostQuery, BlogPostDto>
{
    public async Task<PublicSiteDto> Handle(GetPublicSiteQuery request, CancellationToken cancellationToken)
    {
        var profile = await repository.GetSiteProfileAsync(cancellationToken)
            ?? throw new NotFoundException("Public site profile", "default");
        var branches = await repository.GetPublishedBranchesAsync(cancellationToken);

        return new PublicSiteDto(
            profile.Name,
            profile.Tagline,
            profile.Description,
            profile.Story,
            profile.Eyebrow,
            profile.Phone,
            profile.Email,
            profile.Address,
            profile.City,
            profile.InstagramHandle,
            profile.MemberCount,
            profile.TrainerCount,
            profile.WeeklyClasses,
            profile.Rating,
            profile.FoundedYear,
            profile.Highlights,
            branches.Select(ToDto).ToArray());
    }

    public async Task<IReadOnlyList<BranchDto>> Handle(
        GetBranchesQuery request,
        CancellationToken cancellationToken)
    {
        var branches = await repository.GetPublishedBranchesAsync(cancellationToken);
        return branches.Select(ToDto).ToArray();
    }

    public async Task<BranchDto> Handle(GetBranchQuery request, CancellationToken cancellationToken)
    {
        var branch = await repository.GetPublishedBranchBySlugAsync(request.Slug.Trim(), cancellationToken)
            ?? throw new NotFoundException("Location", request.Slug);
        return ToDto(branch);
    }

    public async Task<IReadOnlyList<MembershipPlanDto>> Handle(
        GetPlansQuery request,
        CancellationToken cancellationToken)
    {
        var plans = await repository.GetPublishedPlansAsync(cancellationToken);
        return plans.Select(ToDto).ToArray();
    }

    public async Task<IReadOnlyList<TrainerDto>> Handle(
        GetTrainersQuery request,
        CancellationToken cancellationToken)
    {
        var trainers = await repository.GetPublishedTrainersAsync(cancellationToken);
        return trainers.Select(ToDto).ToArray();
    }

    public async Task<IReadOnlyList<ClassSessionDto>> Handle(
        GetScheduleQuery request,
        CancellationToken cancellationToken)
    {
        var today = timeProvider.GetLocalNow().Date;
        var from = (request.From ?? today).Date;
        var to = (request.To ?? from.AddDays(14)).Date;

        if (to < from)
            throw new DomainException("The schedule end date cannot be before the start date.");
        if ((to - from).TotalDays > 60)
            throw new DomainException("The schedule window cannot exceed 60 days.");

        var sessions = await repository.GetScheduleAsync(
            from,
            to.AddDays(1).AddTicks(-1),
            request.BranchId,
            cancellationToken);

        return sessions.Select(ToDto).ToArray();
    }

    public async Task<IReadOnlyList<FaqDto>> Handle(
        GetFaqsQuery request,
        CancellationToken cancellationToken)
    {
        var faqs = await repository.GetPublishedFaqsAsync(cancellationToken);
        return faqs.Select(faq => new FaqDto(faq.Id, faq.Category, faq.Question, faq.Answer)).ToArray();
    }

    public async Task<IReadOnlyList<BlogPostSummaryDto>> Handle(
        GetBlogPostsQuery request,
        CancellationToken cancellationToken)
    {
        var posts = await repository.GetPublishedPostsAsync(cancellationToken);
        return posts.Select(post => new BlogPostSummaryDto(
            post.Id,
            post.Slug,
            post.Title,
            post.Excerpt,
            post.Author,
            post.PublishedAt,
            post.ImageUrl)).ToArray();
    }

    public async Task<BlogPostDto> Handle(GetBlogPostQuery request, CancellationToken cancellationToken)
    {
        var post = await repository.GetPublishedPostAsync(request.Slug.Trim(), cancellationToken)
            ?? throw new NotFoundException("Article", request.Slug);

        return new BlogPostDto(
            post.Id,
            post.Slug,
            post.Title,
            post.Excerpt,
            post.Content,
            post.Author,
            post.PublishedAt,
            post.ImageUrl);
    }

    internal static BranchDto ToDto(Branch branch) => new(
        branch.Id,
        branch.Name,
        branch.Slug,
        branch.Neighbourhood,
        branch.Address,
        branch.City,
        branch.Phone,
        branch.OpeningHours,
        branch.Parking,
        branch.Amenities,
        branch.Capacity,
        branch.Latitude,
        branch.Longitude,
        branch.Theme);

    internal static MembershipPlanDto ToDto(MembershipPlan plan) => new(
        plan.Id,
        plan.Name,
        plan.Slug,
        plan.Description,
        plan.Tier,
        plan.Price,
        plan.Currency,
        plan.BillingPeriodMonths,
        BillingLabel(plan.BillingPeriodMonths),
        plan.Features,
        plan.Badge,
        plan.IsFeatured);

    internal static TrainerDto ToDto(Trainer trainer) => new(
        trainer.Id,
        trainer.FullName,
        trainer.Slug,
        trainer.RoleTitle,
        trainer.Bio,
        trainer.Specializations,
        trainer.Certifications,
        trainer.YearsExperience,
        trainer.BranchName,
        trainer.PhotoUrl);

    internal static ClassSessionDto ToDto(ClassSession session) => new(
        session.Id,
        session.Name,
        session.Description,
        session.Category,
        session.Intensity,
        session.TrainerId,
        session.TrainerName,
        session.StartsAt,
        session.DurationMinutes,
        session.Capacity,
        session.Room,
        session.BranchId,
        session.BranchName);

    private static string BillingLabel(int months) => months switch
    {
        1 => "per month",
        3 => "per quarter",
        6 => "per 6 months",
        12 => "per year",
        _ => $"per {months} months",
    };
}
