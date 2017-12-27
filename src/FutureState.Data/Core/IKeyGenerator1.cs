using System;

namespace FutureState.Data.Core
{
    /// <summary>
    ///     Responsible for generating a new id for a given key type.
    /// </summary>
    // ReSharper disable once UnusedTypeParameter
    public interface IKeyGenerator<TEntity, out TKey> : IKeyGenerator // first type param required to distinguish instances in app domain
    {
        TKey GetNew();

        Type EntityType { get; }
    }
}