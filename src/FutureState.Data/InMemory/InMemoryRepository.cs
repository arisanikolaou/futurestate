using System;
using System.Collections.Generic;
using System.Linq;
using FutureState.Data.KeyBinders;
using FutureState.Data.Keys;

namespace FutureState.Data
{
    //an - replaced combguid with guid.newguid

    /// <summary>
    ///     InMemoryRepository version that uses Guid as the PK, AttributeKeyBinder and GuidGenerator.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class InMemoryRepository<TEntity> :
        InMemoryRepository<TEntity, Guid>,
        IRepositoryLinq<TEntity>,
        IRepository<TEntity>,
        IGetter<TEntity>,
        IReader<TEntity>,
        ILinqReader<TEntity>
    {
        public InMemoryRepository(
            IEntityIdProvider<TEntity, Guid> idGenerator,
            IEntityKeyBinder<TEntity, Guid> keyBinder,
            IEnumerable<TEntity> items)
            :
            base(idGenerator, keyBinder, items)
        {
        }

        public InMemoryRepository(IEnumerable<TEntity> items)
            : base(
                new EntityIdProvider<TEntity, Guid>(new KeyGetter<Guid>(Guid.NewGuid)),
                new AttributeKeyBinder<TEntity, Guid>(),
                items)
        {
        }

        public InMemoryRepository()
            : this(Enumerable.Empty<TEntity>())
        {
        }
    }
}