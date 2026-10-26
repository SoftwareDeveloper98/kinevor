using Kinevor.Platform.Application.Common;
using MediatR;

namespace Kinevor.Modules.Trainers.Application;

/// <summary>Paged trainer search.</summary>
/// <param name="Search">Free-text filter over name and e-mail, or null.</param>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Page size.</param>
public record GetTrainersQuery(string? Search, int Page = 1, int PageSize = 20)
    : IRequest<PagedResult<TrainerSummaryDto>>;

/// <summary>Handles <see cref="GetTrainersQuery"/>.</summary>
public class GetTrainersHandler(ITrainerRepository repo)
    : IRequestHandler<GetTrainersQuery, PagedResult<TrainerSummaryDto>>
{
    /// <summary>Returns the matching page of trainers.</summary>
    public async Task<PagedResult<TrainerSummaryDto>> Handle(GetTrainersQuery q, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(q);

        var page = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var (items, total) = await repo.SearchAsync(q.Search, page, pageSize, onlyActive: false, ct);
        return new PagedResult<TrainerSummaryDto>(
            items.Select(TrainerMapper.ToSummary).ToList(), total, page, pageSize);
    }
}
