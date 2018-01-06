using System;

namespace FutureState.Data
{
    /// <summary>
    ///     An entity set capable of performing linq queries.
    /// </summary>
    public class EntitySetLinq<TEntity, TKey> : EntitySet<TEntity, TKey>
    {
        private readonly Func<ISession, IRepositoryLinq<TEntity, TKey>> _getRepositoryLinq;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="getRepository"></param>
        public EntitySetLinq(
            UnitOfWork unitOfWork,
            Func<ISession, IRepositoryLinq<TEntity, TKey>> getRepository) : base(unitOfWork, getRepository)
        {
            _getRepositoryLinq = getRepository;
        }

        public ILinqReader<TEntity, TKey> LinqReader => _getRepositoryLinq.Invoke(_unitOfWork.Session);

        public IBulkDeleter<TEntity, TKey> BulkDeleter => _getRepositoryLinq.Invoke(_unitOfWork.Session);
    }
}