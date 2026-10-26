namespace Kinevor.Platform.Application.Common;

/// <summary>Paged list envelope shared by every list endpoint.</summary>
/// <typeparam name="T">Item type.</typeparam>
public record PagedResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize)
{
    /// <summary>Total number of pages for the current page size.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>Whether a next page exists.</summary>
    public bool HasNext => Page < TotalPages;

    /// <summary>Whether a previous page exists.</summary>
    public bool HasPrev => Page > 1;
}
