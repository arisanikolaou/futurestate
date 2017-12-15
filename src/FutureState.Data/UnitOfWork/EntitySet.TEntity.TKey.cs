using System;

namespace FutureState.Data
{
    /// <summary>
    ///     Manages read/write operations to a set of entites given a unit of work.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to read/write.</typeparam>
    /// <typeparam name="TKey">The entity's key type.</typeparam>
    public class EntitySet<TEntity, TKey>
    {
        protected readonly DataSessionManager _unitOfWork;
        readonly Func<ISession, IRepository<TEntity, TKey>> _getRepository;
        readonly EntitySetWriter<TEntity, TKey> _entitySetWriter;

        /// <summary>
        ///     Exposes methods to update the entities.
        /// </summary>
        public IWriter<TEntity, TKey> Writer => _entitySetWriter;

        /// <summary>
        ///     Exposes implementations to read the set of entities.
        /// </summary>
        public IReader<TEntity, TKey> Reader => _getRepository.Invoke(_unitOfWork.Session);

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="unitOfWork"></param>
        /// <param name="getRepository"></param>
        public EntitySet(
            UnitOfWork unitOfWork,
            Func<ISession, IRepository<TEntity, TKey>> getRepository)
        {
            Guard.ArgumentNotNull(unitOfWork,nameof(unitOfWork));
            Guard.ArgumentNotNull(getRepository, nameof(getRepository));

            _getRepository = getRepository;

            _entitySetWriter = new EntitySetWriter<TEntity, TKey>(unitOfWork, getRepository);

            _unitOfWork = unitOfWork;
        }
    }
}
