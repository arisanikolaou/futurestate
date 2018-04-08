using System;
using System.Linq.Expressions;

namespace FutureState.Data
{
    /// <summary>
    ///     An entity set capable of performing linq queries.
    /// </summary>
    public class EntitySetLinq<TEntity, TKey> : EntitySet<TEntity, TKey>
    {
        private readonly Func<ISession, IRepositoryLinq<TEntity, TKey>> _getRepositoryLinq;

        readonly EntitySetBulkdDeleter<TEntity, TKey> _bulkDeleter;

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

            _bulkDeleter = new EntitySetBulkdDeleter<TEntity, TKey>(unitOfWork, getRepository);
        }

        public ILinqReader<TEntity, TKey> LinqReader => _getRepositoryLinq.Invoke(_unitOfWork.Session);

        public IBulkDeleter<TEntity, TKey> BulkDeleter => _bulkDeleter;
    }

    internal class EntitySetBulkdDeleter<TEntity, TKey> : IBulkDeleter<TEntity, TKey>
    {
        private readonly Func<ISession, IRepositoryLinq<TEntity, TKey>> _repositoryFunc;
        private readonly UnitOfWork _uow;

        private IBulkDeleter<TEntity, TKey> Writer => _repositoryFunc.Invoke(_uow.Session);

        internal EntitySetBulkdDeleter(UnitOfWork work, Func<ISession, IRepositoryLinq<TEntity, TKey>> repositoryFunc)
        {
            Guard.ArgumentNotNull(repositoryFunc, "repositoryFunc");

            // internal class assume valid inputs
            _uow = work;
            _repositoryFunc = repositoryFunc;
        }

        public void Delete(Expression<Func<TEntity, bool>> predicate)
        {
            ValidateAction();

            _uow._executionQueue.Enqueue(() => Writer.Delete(predicate));
        }

        private void ValidateAction()
        {
            if (_uow.IsDisposed)
                throw new InvalidOperationException(
                    "The underlying session has been disposed and can no longer be written or read from.");
        }
    }
}