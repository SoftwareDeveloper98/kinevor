using Kinevor.Modules.PublicSite.Application;
using Kinevor.Modules.PublicSite.Domain;
using Microsoft.EntityFrameworkCore;

namespace Kinevor.Modules.PublicSite.EntityFrameworkCore;

/// <summary>EF Core implementation of the public-site repository.</summary>
public sealed class PublicSiteRepository(PublicSiteDbContext db) : IPublicSiteRepository
{
    /// <summary>Returns the seeded site profile.</summary>
    public Task<SiteProfile?> GetSiteProfileAsync(CancellationToken cancellationToken) =>
        db.SiteProfiles.AsNoTracking().FirstOrDefaultAsync(cancellationToken);

    /// <summary>Returns published branches in display order.</summary>
    public async Task<IReadOnlyList<Branch>> GetPublishedBranchesAsync(CancellationToken cancellationToken) =>
        await db.Branches.AsNoTracking()
            .Where(branch => branch.IsPublished)
            .OrderBy(branch => branch.SortOrder)
            .ToListAsync(cancellationToken);

    /// <summary>Returns one published branch by slug.</summary>
    public Task<Branch?> GetPublishedBranchBySlugAsync(
        string slug,
        CancellationToken cancellationToken) =>
        db.Branches.AsNoTracking()
            .FirstOrDefaultAsync(branch => branch.IsPublished && branch.Slug == slug, cancellationToken);

    /// <summary>Returns one published branch by identifier.</summary>
    public Task<Branch?> GetPublishedBranchAsync(Guid id, CancellationToken cancellationToken) =>
        db.Branches.AsNoTracking()
            .FirstOrDefaultAsync(branch => branch.IsPublished && branch.Id == id, cancellationToken);

    /// <summary>Returns published plans in display order.</summary>
    public async Task<IReadOnlyList<MembershipPlan>> GetPublishedPlansAsync(
        CancellationToken cancellationToken) =>
        await db.MembershipPlans.AsNoTracking()
            .Where(plan => plan.IsPublished)
            .OrderBy(plan => plan.SortOrder)
            .ToListAsync(cancellationToken);

    /// <summary>Returns published trainers in display order.</summary>
    public async Task<IReadOnlyList<Trainer>> GetPublishedTrainersAsync(
        CancellationToken cancellationToken) =>
        await db.Trainers.AsNoTracking()
            .Where(trainer => trainer.IsPublished)
            .OrderBy(trainer => trainer.SortOrder)
            .ToListAsync(cancellationToken);

    /// <summary>Returns published classes in the requested window.</summary>
    public async Task<IReadOnlyList<ClassSession>> GetScheduleAsync(
        DateTime from,
        DateTime to,
        Guid? branchId,
        CancellationToken cancellationToken)
    {
        var query = db.ClassSessions.AsNoTracking()
            .Where(session => session.IsPublished && session.StartsAt >= from && session.StartsAt <= to);

        if (branchId.HasValue)
            query = query.Where(session => session.BranchId == branchId.Value);

        return await query
            .OrderBy(session => session.StartsAt)
            .ThenBy(session => session.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Returns one published class by identifier.</summary>
    public Task<ClassSession?> GetPublishedClassAsync(Guid id, CancellationToken cancellationToken) =>
        db.ClassSessions.AsNoTracking()
            .FirstOrDefaultAsync(session => session.IsPublished && session.Id == id, cancellationToken);

    /// <summary>Returns published FAQs in display order.</summary>
    public async Task<IReadOnlyList<FaqEntry>> GetPublishedFaqsAsync(
        CancellationToken cancellationToken) =>
        await db.FaqEntries.AsNoTracking()
            .Where(faq => faq.IsPublished)
            .OrderBy(faq => faq.SortOrder)
            .ToListAsync(cancellationToken);

    /// <summary>Returns published articles newest first.</summary>
    public async Task<IReadOnlyList<BlogPost>> GetPublishedPostsAsync(
        CancellationToken cancellationToken) =>
        await db.BlogPosts.AsNoTracking()
            .Where(post => post.IsPublished)
            .OrderByDescending(post => post.PublishedAt)
            .ToListAsync(cancellationToken);

    /// <summary>Returns one published article by slug.</summary>
    public Task<BlogPost?> GetPublishedPostAsync(
        string slug,
        CancellationToken cancellationToken) =>
        db.BlogPosts.AsNoTracking()
            .FirstOrDefaultAsync(post => post.IsPublished && post.Slug == slug, cancellationToken);

    /// <summary>Checks for an open trial request matching the contact and date.</summary>
    public Task<bool> TrialExistsAsync(
        string email,
        DateTime? preferredDate,
        CancellationToken cancellationToken)
    {
        var query = db.TrialBookings.AsNoTracking()
            .Where(trial => trial.Email == email &&
                            (trial.Status == LeadLifecycle.New || trial.Status == LeadLifecycle.Contacted));

        if (preferredDate.HasValue)
            query = query.Where(trial => trial.PreferredDate == preferredDate.Value);

        return query.AnyAsync(cancellationToken);
    }

    /// <summary>Returns every website enquiry.</summary>
    public async Task<IReadOnlyList<Lead>> GetLeadsAsync(CancellationToken cancellationToken) =>
        await db.Leads.AsNoTracking().ToListAsync(cancellationToken);

    /// <summary>Returns every trial request.</summary>
    public async Task<IReadOnlyList<TrialBooking>> GetTrialBookingsAsync(
        CancellationToken cancellationToken) =>
        await db.TrialBookings.AsNoTracking().ToListAsync(cancellationToken);

    /// <summary>Stages a website enquiry.</summary>
    public void AddLead(Lead lead) => db.Leads.Add(lead);

    /// <summary>Stages a trial request.</summary>
    public void AddTrialBooking(TrialBooking trialBooking) => db.TrialBookings.Add(trialBooking);

    /// <summary>Persists staged changes.</summary>
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken) =>
        db.SaveChangesAsync(cancellationToken);
}
