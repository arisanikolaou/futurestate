using System.Collections.Generic;

namespace FutureState.Flow
{
    /// <summary>
    ///     Reads data of a given type from a given data source.
    /// </summary>
    /// <typeparam name="TEntity">The entity type read from the underlying data source.</typeparam>
    public interface IReader<out TEntity> : IReader
    {
        IEnumerable<TEntity> Read(string dataSource);
    }

    /// <summary>
    ///     Reads data from an incoming data source.
    /// </summary>
    public interface IReader
    {
    }
}