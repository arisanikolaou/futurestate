using System;

namespace FutureState.Data
{
    /// <summary>
    ///     Adapts a repository to a unit of work to read entities within a consistent data session.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to read.</typeparam>
    /// <typeparam name="TKey">The entity type key.</typeparam>
    public class EntitySetReader<TEntity, TKey> : EntitySetReaderBase<ILinqReader<TEntity, TKey>>
    {
        internal EntitySetReader(Func<ISession, ILinqReader<TEntity, TKey>> linqReaderFunc,
            DataSessionManager dataSessionManager) : base(dataSessionManager, linqReaderFunc)
        {
        }
    }

    /// <summary>
    ///     Adapts a repository to a unit of work to read entities within a consistent data session.
    /// </summary>
    public class EntitySetReaderBase<TQuery> where TQuery : class
    {
        private readonly DataSessionManager _dataSessionManager;

        internal EntitySetReaderBase(DataSessionManager dataSessionManager,
            Func<ISession, TQuery> readerFunc)
        {
            _dataSessionManager = dataSessionManager;

            ReaderFunc = readerFunc;
        }

        internal Func<ISession, TQuery> ReaderFunc { get; }

        internal TQuery Reader => ReaderFunc?.Invoke(_dataSessionManager.Session);
    }
}