using Kinevor.Modules.PublicSite.Domain;
using Kinevor.Platform.Application.Exceptions;
using MediatR;

namespace Kinevor.Modules.PublicSite.Application;

/// <summary>Stores a validated free-trial request.</summary>
public sealed record CreateTrialCommand(TrialRequest Request) : IRequest<TrialCreatedDto>;

/// <summary>Stores a validated website enquiry.</summary>
public sealed record CreateLeadCommand(LeadRequest Request) : IRequest<LeadCreatedDto>;

/// <summary>Loads the combined staff lead inbox.</summary>
public sealed record GetLeadsQuery(
    string? Kind,
    string? Status,
    string? Search,
    int Page,
    int PageSize) : IRequest<PagedResult<LeadDto>>;

internal sealed class CreateTrialHandler(IPublicSiteRepository repository, TimeProvider timeProvider)
    : IRequestHandler<CreateTrialCommand, TrialCreatedDto>
{
    public async Task<TrialCreatedDto> Handle(
        CreateTrialCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        ClassSession? selectedClass = null;

        if (request.ClassSessionId is { } classId)
        {
            selectedClass = await repository.GetPublishedClassAsync(classId, cancellationToken)
                ?? throw new NotFoundException("Class session", classId);
        }

        Branch? selectedBranch = null;
        if (request.BranchId is { } branchId)
        {
            selectedBranch = await repository.GetPublishedBranchAsync(branchId, cancellationToken)
                ?? throw new NotFoundException("Location", branchId);
        }

        var preferredDate = selectedClass?.StartsAt ?? request.PreferredDate;
        var booking = TrialBooking.Create(
            timeProvider.GetLocalNow().DateTime,
            request.FirstName,
            request.LastName,
            request.Email,
            request.Phone,
            preferredDate,
            selectedClass?.Id ?? request.ClassSessionId,
            selectedClass?.Name,
            selectedClass?.BranchId ?? selectedBranch?.Id ?? request.BranchId,
            selectedClass?.BranchName ?? selectedBranch?.Name,
            request.Goal,
            request.Notes,
            request.ConsentToContact);

        if (await repository.TrialExistsAsync(booking.Email, booking.PreferredDate, cancellationToken))
            throw new ConflictException("An open trial request already exists for this e-mail and date.");

        repository.AddTrialBooking(booking);
        await repository.SaveChangesAsync(cancellationToken);

        return new TrialCreatedDto(
            booking.Id,
            "Your trial request is in. Our membership team will confirm your session shortly.");
    }
}

internal sealed class CreateLeadHandler(IPublicSiteRepository repository)
    : IRequestHandler<CreateLeadCommand, LeadCreatedDto>
{
    public async Task<LeadCreatedDto> Handle(
        CreateLeadCommand command,
        CancellationToken cancellationToken)
    {
        var request = command.Request;
        var lead = Lead.Create(
            request.Name,
            request.Email,
            request.Phone,
            request.Interest,
            request.Message,
            request.ConsentToContact);

        repository.AddLead(lead);
        await repository.SaveChangesAsync(cancellationToken);

        return new LeadCreatedDto(
            lead.Id,
            "Thanks for reaching out. A Kinevor team member will reply within one business day.");
    }
}

internal sealed class GetLeadsHandler(IPublicSiteRepository repository)
    : IRequestHandler<GetLeadsQuery, PagedResult<LeadDto>>
{
    private static readonly HashSet<string> Kinds =
        new(StringComparer.OrdinalIgnoreCase) { "Trial", "Enquiry" };

    public async Task<PagedResult<LeadDto>> Handle(
        GetLeadsQuery query,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);
        var kind = string.IsNullOrWhiteSpace(query.Kind) ? null : query.Kind.Trim();
        if (kind is not null && !Kinds.Contains(kind))
            throw new DomainException("kind must be Trial or Enquiry.");

        LeadLifecycle? status = null;
        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            if (!Enum.TryParse<LeadLifecycle>(query.Status.Trim(), true, out var parsed))
                throw new DomainException("status must be New, Contacted, Booked or Closed.");
            status = parsed;
        }

        var search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        var leads = await repository.GetLeadsAsync(cancellationToken);
        var trials = await repository.GetTrialBookingsAsync(cancellationToken);
        var rows = new List<LeadDto>(leads.Count + trials.Count);

        if (kind is null || kind.Equals("Enquiry", StringComparison.OrdinalIgnoreCase))
        {
            rows.AddRange(leads.Select(lead => new LeadDto(
                lead.Id,
                "Enquiry",
                lead.Name,
                lead.Email,
                lead.Phone,
                lead.Interest,
                lead.Message,
                null,
                null,
                null,
                lead.Status.ToString(),
                lead.CreatedAt)));
        }

        if (kind is null || kind.Equals("Trial", StringComparison.OrdinalIgnoreCase))
        {
            rows.AddRange(trials.Select(trial => new LeadDto(
                trial.Id,
                "Trial",
                trial.FullName,
                trial.Email,
                trial.Phone,
                trial.Goal ?? "Free trial",
                trial.Notes,
                trial.PreferredDate,
                trial.ClassName,
                trial.BranchName,
                trial.Status.ToString(),
                trial.CreatedAt)));
        }

        IEnumerable<LeadDto> filtered = rows;
        if (status is not null)
            filtered = filtered.Where(row => row.Status == status.Value.ToString());
        if (search is not null)
        {
            filtered = filtered.Where(row =>
                row.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                row.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (row.Interest?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (row.Message?.Contains(search, StringComparison.OrdinalIgnoreCase) ?? false));
        }

        var ordered = filtered.OrderByDescending(row => row.CreatedAt).ToArray();
        var total = ordered.Length;
        var totalPages = total == 0 ? 0 : (int)Math.Ceiling(total / (double)pageSize);
        var items = ordered.Skip((page - 1) * pageSize).Take(pageSize).ToArray();

        return new PagedResult<LeadDto>(
            items,
            total,
            page,
            pageSize,
            totalPages,
            page < totalPages,
            page > 1 && totalPages > 0);
    }
}
