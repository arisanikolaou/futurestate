using System;
using System.Diagnostics;

namespace FutureState.Data
{
    /// <summary>
    ///     A unit of work that operates against a given entity type.
    /// </summary>
    /// <typeparam name="TEntity">The entity to save, add or remove.</typeparam>
    /// <typeparam name="TKey">The entity's key type.</typeparam>
    public interface IUnitOfWorkLinq<TEntity, TKey> : IUnitOfWork where TEntity : class, IEntity<TKey>
    {
        EntitySetLinq<TEntity, TKey> EntitySet { get; }
    }

    /// <summary>
    ///     A unit of work capable of performing linq queries on the underlying data set.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    [DebuggerDisplay("{ToString()}")]
    public class UnitOfWorkLinq<TEntity, TKey> : UnitOfWork, IUnitOfWorkLinq<TEntity, TKey>
        where TEntity : class, IEntity<TKey>
    {
        protected EntitySetLinq<TEntity, TKey> _entitySet;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="getRepository">The function to produce the repository.</param>
        /// <param name="sessionFactory">The factory to produce sessions to pass into the given repository.</param>
        /// <param name="policy">The transaction commit policy.</param>
        public UnitOfWorkLinq(
            Func<ISession, IRepositoryLinq<TEntity, TKey>> getRepository,
            ISessionFactory sessionFactory,
            ICommitPolicy policy = null)
            : base(sessionFactory, policy)
        {
            Guard.ArgumentNotNull(getRepository, nameof(getRepository));

            _entitySet = new EntitySetLinq<TEntity, TKey>(this, getRepository);
        }

        /// <summary>
        ///     The entity set to read/write and query from.
        /// </summary>
        public EntitySetLinq<TEntity, TKey> EntitySet => _entitySet;

        /// <summary>
        ///     Gets the display name of the unit of work.
        /// </summary>
        public override string ToString()
        {
            return $"{GetType().Name}<{typeof(TEntity).Name},{typeof(TKey).Name}> {GetSessionFactory()}";
        }
    }
}