using System.Linq.Expressions;

namespace Kinevor.Platform.Domain.Specifications;

/// <summary>Convenience base for specifications: subclasses supply the criteria and optionally add ordering or paging.</summary>
/// <typeparam name="T">Entity type the specification filters.</typeparam>
public abstract class BaseSpecification<T> : ISpecification<T>
{
    private readonly List<Expression<Func<T, object>>> _includes = [];

    /// <summary>Creates a specification with the given filter predicate.</summary>
    protected BaseSpecification(Expression<Func<T, bool>> criteria) => Criteria = criteria;

    /// <summary>Creates a specification that matches every entity.</summary>
    protected BaseSpecification() => Criteria = _ => true;

    /// <summary>Filter predicate applied to the queryable set.</summary>
    public Expression<Func<T, bool>> Criteria { get; }

    /// <summary>Includes requested by the specification, applied to the queryable set.</summary>
    public IReadOnlyList<Expression<Func<T, object>>> Includes => _includes;

    /// <summary>Optional ascending sort selector applied after filtering.</summary>
    public Expression<Func<T, object>>? OrderBy { get; private set; }

    /// <summary>Optional descending sort selector applied after filtering.</summary>
    public Expression<Func<T, object>>? OrderByDescending { get; private set; }

    /// <summary>Number of items to take; paging is active when set.</summary>
    public int? Take { get; private set; }

    /// <summary>Number of items to skip; paging is active when set.</summary>
    public int? Skip { get; private set; }

    /// <summary>Adds an include expression for eager loading.</summary>
    protected void AddInclude(Expression<Func<T, object>> includeExpression) => _includes.Add(includeExpression);

    /// <summary>Sets the ascending sort selector.</summary>
    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression) => OrderBy = orderByExpression;

    /// <summary>Sets the descending sort selector.</summary>
    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression) => OrderByDescending = orderByDescExpression;

    /// <summary>Activates paging with the given offset and page size.</summary>
    protected void ApplyPaging(int skip, int take) { Skip = skip; Take = take; }
}
