namespace Kinevor.Platform.Application.Common;

/// <summary>
/// Current branch for multi-branch tenancy. <see langword="null"/> means all branches (Admin default);
/// resolved per request from the user's claim or the <c>X-Branch-Id</c> header.
/// </summary>
public interface IBranchContext
{
    /// <summary>Branch the current request is scoped to, when any.</summary>
    Guid? BranchId { get; }
}
