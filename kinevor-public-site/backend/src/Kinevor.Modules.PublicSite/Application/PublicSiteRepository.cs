using Kinevor.Modules.PublicSite.Domain;

namespace Kinevor.Modules.PublicSite.Application;

/// <summary>Persistence contract owned by the public-site module.</summary>
public interface IPublicSiteRepository
{
    /// <summary>Returns the singleton site profile, when seeded.</summary>
    Task<SiteProfile?> GetSiteProfileAsync(CancellationToken cancellationToken);

    /// <summary>Returns published branches in display order.</summary>
    Task<IReadOnlyList<Branch>> GetPublishedBranchesAsync(CancellationToken cancellationToken);

    /// <summary>Returns one published branch by slug.</summary>
    Task<Branch?> GetPublishedBranchBySlugAsync(string slug, CancellationToken cancellationToken);

    /// <summary>Returns one published branch by identifier.</summary>
    Task<Branch?> GetPublishedBranchAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns published membership plans in display order.</summary>
    Task<IReadOnlyList<MembershipPlan>> GetPublishedPlansAsync(CancellationToken cancellationToken);

    /// <summary>Returns published trainers in display order.</summary>
    Task<IReadOnlyList<Trainer>> GetPublishedTrainersAsync(CancellationToken cancellationToken);

    /// <summary>Returns published classes within the requested window.</summary>
    Task<IReadOnlyList<ClassSession>> GetScheduleAsync(
        DateTime from,
        DateTime to,
        Guid? branchId,
        CancellationToken cancellationToken);

    /// <summary>Returns one published class by identifier.</summary>
    Task<ClassSession?> GetPublishedClassAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Returns published FAQs in display order.</summary>
    Task<IReadOnlyList<FaqEntry>> GetPublishedFaqsAsync(CancellationToken cancellationToken);

    /// <summary>Returns published blog posts, newest first.</summary>
    Task<IReadOnlyList<BlogPost>> GetPublishedPostsAsync(CancellationToken cancellationToken);

    /// <summary>Returns one published blog post by slug.</summary>
    Task<BlogPost?> GetPublishedPostAsync(string slug, CancellationToken cancellationToken);

    /// <summary>Checks for an existing open trial request with the same contact and date.</summary>
    Task<bool> TrialExistsAsync(string email, DateTime? preferredDate, CancellationToken cancellationToken);

    /// <summary>Returns all website enquiries.</summary>
    Task<IReadOnlyList<Lead>> GetLeadsAsync(CancellationToken cancellationToken);

    /// <summary>Returns all trial requests.</summary>
    Task<IReadOnlyList<TrialBooking>> GetTrialBookingsAsync(CancellationToken cancellationToken);

    /// <summary>Stages a website enquiry.</summary>
    void AddLead(Lead lead);

    /// <summary>Stages a trial request.</summary>
    void AddTrialBooking(TrialBooking trialBooking);

    /// <summary>Persists staged changes.</summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
