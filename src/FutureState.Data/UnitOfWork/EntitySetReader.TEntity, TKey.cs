using System;

namespace FutureState.Data
{
    public class EntitySetReader<TEntity, TKey> : EntitySetReaderBase<ILinqReader<TEntity, TKey>>
    {
        internal EntitySetReader(Func<ISession, ILinqReader<TEntity, TKey>> linqReaderFunc,
            DataSessionManager dataSessionManager) : base(dataSessionManager, linqReaderFunc)
        {

        }
    }

    public class EntitySetReaderBase<TQuery>  where TQuery : class
    {
        readonly DataSessionManager _dataSessionManager;

        internal EntitySetReaderBase(DataSessionManager dataSessionManager,
            Func<ISession, TQuery> readerFunc)
        {
            _dataSessionManager = dataSessionManager;

            ReaderFunc = readerFunc;
        }

        internal Func<ISession, TQuery> ReaderFunc { get; }

        internal TQuery Reader => ReaderFunc?.Invoke(this._dataSessionManager.Session);
    }
}