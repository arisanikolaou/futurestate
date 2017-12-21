using System;

namespace FutureState.Data.Core
{
    /// <summary>
    ///     Generic key generator for a given key type.
    /// </summary>
    public class KeyGenerator<TEntity, TKey> : IKeyGenerator<TEntity, TKey>
    {
        private readonly Func<TKey> _getGet;

        public Type EntityType { get; } = typeof(TEntity);

        public KeyGenerator(Func<TKey> getGet)
        {
            Guard.ArgumentNotNull(getGet, nameof(getGet));

            _getGet = getGet;
        }

        public TKey GetNew()
        {
            return _getGet();
        }
    }
}