using System;
using System.Diagnostics;

namespace FutureState.Data
{
    /// <summary>
    /// Unit of work to read/write the state of a given set of entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity to add/remove or insert.</typeparam>
    /// <typeparam name="TKey">The entity's key.</typeparam>
    public interface IUnitOfWork<TEntity,TKey> : IUnitOfWork
        where TEntity : class, IEntity<TKey>
    {
        EntitySet<TEntity, TKey> EntitySet { get; }
    }

    /// <summary>
    /// Unit of work to read/write the state of a given set of entities.
    /// </summary>
    /// <typeparam name="TEntity">The entity to add/remove or insert.</typeparam>
    /// <typeparam name="TKey">The entity's key.</typeparam>
    [DebuggerDisplay("{ToString()}")]
    public class UnitOfWork<TEntity, TKey> : UnitOfWork, IUnitOfWork<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        protected EntitySet<TEntity, TKey> _entitySet;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="getRepository">The function to produce the repository.</param>
        /// <param name="sessionFactory">The factory to produce sessions to pass into the given repository.</param>
        /// <param name="policy">The transaction commit policy.</param>
        public UnitOfWork(
            Func<ISession, IRepository<TEntity, TKey>> getRepository,
            ISessionFactory sessionFactory,
            ICommitPolicy policy = null)
            : base(sessionFactory, policy)
        {
            Guard.ArgumentNotNull(getRepository, nameof(getRepository));

            _entitySet = new EntitySet<TEntity, TKey>(this, getRepository);
        }


        public EntitySet<TEntity, TKey> EntitySet => _entitySet;

        /// <summary>
        /// Gets the display name of the unit of work.
        /// </summary>
        public override string ToString()
        {
            return $"{GetType().Name}<{typeof(TEntity).Name},{typeof(TKey).Name}> {GetSessionFactory()}";
        }
    }
}