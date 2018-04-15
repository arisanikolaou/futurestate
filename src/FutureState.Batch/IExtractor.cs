using System.Collections.Generic;

namespace FutureState.Batch
{
    /// <summary>
    ///     Extracts data from a data source into a stream.
    /// </summary>
    public interface IExtractor
    {
    }

    /// <summary>
    ///     Extracts data of a given type from a data source into a stream.
    /// </summary>
    public interface IExtractor<out TEntity>
    {
        IEnumerable<TEntity> Read();
    }
}
