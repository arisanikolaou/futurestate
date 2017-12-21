using System;

namespace FutureState.Data.Core
{
    public class DefaultKeyGenerator<TEntity, TKey> : IKeyGenerator<TEntity, TKey>
    {
        public Type EntityType { get; } = typeof(TEntity);

        public TKey GetNew() => default(TKey);
    }
}