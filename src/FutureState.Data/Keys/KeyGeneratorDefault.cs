using System;

namespace FutureState.Data
{
    // essentially no-op key generator

    public class KeyGeneratorDefault<TEntity, TKey> : IKeyGenerator<TEntity, TKey>
    {
        public Type EntityType { get; } = typeof(TEntity);

        public TKey GetNew()
        {
            return default(TKey);
        }
    }
}