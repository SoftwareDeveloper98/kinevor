namespace Kinevor.Platform.Application.Common;

/// <summary>Full repository contract for an aggregate: reads plus staged writes.</summary>
/// <typeparam name="T">Aggregate type.</typeparam>
public interface IRepository<T> : IReadRepository<T>, IWriteRepository<T> where T : class;
