using System.Collections.Generic;

namespace FutureState.Batch
{
    /// <summary>
    ///     Extracts data from a data source into a stream.
    /// </summary>
    public interface IExtractor
    {
        /// <summary>
        ///     Gets the data source file path to read from.
        /// </summary>
        string Uri { get; set; }
    }

    /// <summary>
    ///     Extracts data of a given type from a data source into a stream.
    /// </summary>
    public interface IExtractor<out TEntity> : IExtractor
    {
        IEnumerable<TEntity> Read();
    }
}
