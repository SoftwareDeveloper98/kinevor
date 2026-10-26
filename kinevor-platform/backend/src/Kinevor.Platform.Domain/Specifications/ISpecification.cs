using System.Linq.Expressions;

namespace Kinevor.Platform.Domain.Specifications;

/// <summary>Composable query contract: predicate plus paging and ordering hints for the repository layer.</summary>
/// <typeparam name="T">Entity type the specification filters.</typeparam>
public interface ISpecification<T>
{
    /// <summary>Filter predicate applied to the queryable set.</summary>
    Expression<Func<T, bool>> Criteria { get; }

    /// <summary>Includes requested by the specification, applied to the queryable set.</summary>
    IReadOnlyList<Expression<Func<T, object>>> Includes { get; }

    /// <summary>Optional ascending sort selector applied after filtering.</summary>
    Expression<Func<T, object>>? OrderBy { get; }

    /// <summary>Optional descending sort selector applied after filtering.</summary>
    Expression<Func<T, object>>? OrderByDescending { get; }

    /// <summary>Number of items to take; paging is active when set.</summary>
    int? Take { get; }

    /// <summary>Number of items to skip; paging is active when set.</summary>
    int? Skip { get; }
}
