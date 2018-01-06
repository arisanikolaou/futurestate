using System;

namespace FutureState.Data
{
    public class KeyGeneratorDefault<TEntity, TKey> : IKeyGenerator<TEntity, TKey>
    {
        public Type EntityType { get; } = typeof(TEntity);

        public TKey GetNew()
        {
            return default(TKey);
        }
    }
}