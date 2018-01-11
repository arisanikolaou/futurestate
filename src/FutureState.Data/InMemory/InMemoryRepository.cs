using System;
using System.Collections.Generic;
using System.Linq;

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
        IRepository<TEntity>
    {
        public InMemoryRepository(
            IKeyProvider<TEntity, Guid> keyProvider,
            IKeyBinder<TEntity, Guid> keyBinder,
            IEnumerable<TEntity> items)
            :
            base(keyProvider, keyBinder, items)
        {
        }

        public InMemoryRepository(IEnumerable<TEntity> items)
            : base(
                new KeyProvider<TEntity, Guid>(new KeyGenerator<TEntity, Guid>(Guid.NewGuid)),
                new KeyBinderFromAttributes<TEntity, Guid>(),
                items)
        {
        }

        public InMemoryRepository()
            : this(Enumerable.Empty<TEntity>())
        {
        }
    }
}