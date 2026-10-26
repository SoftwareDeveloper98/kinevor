namespace Kinevor.Platform.Application.Common;

/// <summary>Write side of the repository contract; changes are persisted by the unit of work.</summary>
/// <typeparam name="T">Aggregate type.</typeparam>
public interface IWriteRepository<T> where T : class
{
    /// <summary>Stages a new aggregate for insertion.</summary>
    void Add(T entity);

    /// <summary>Stages an existing aggregate for update.</summary>
    void Update(T entity);

    /// <summary>Stages an aggregate for removal.</summary>
    void Remove(T entity);
}
