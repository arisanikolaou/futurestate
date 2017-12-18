#region

using System;

#endregion

namespace FutureState.Data
{
    /// <summary>
    ///     A handler to add responsibilities to a given linq reader.
    /// </summary>
    public interface ILinqReaderHandler
    {
        /// <summary>
        ///     Wraps responsibilities around a given reader.
        /// </summary>
        Func<ISession, ILinqReader<TEntity, TKey>> HandleReader<TEntity, TKey>(
            Func<ISession, ILinqReader<TEntity, TKey>> getLinqReader)
            where TEntity : class, IEntity<TKey>, new();
    }
}