using System;

namespace FutureState.Data
{
    /// <summary>
    ///     A handler to implement a chain of responsibility for any repository,
    ///     or Ilinqreader implementation.
    /// </summary>
    public interface IRepositoryHandler : ILinqReaderHandler
    {
        /// <summary>
        ///     Wraps responsibilties around a given repository.
        /// </summary>
        Func<ISession, IRepositoryLinq<TEntity, TKey>> HandleRepository<TEntity, TKey>(
            Func<ISession, IRepositoryLinq<TEntity, TKey>> getRepository)
            where TEntity : class, IEntity<TKey>, new();
    }
}