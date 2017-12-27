namespace FutureState.Data
{
    /// <summary>
    ///     An object whose state is expected to persist across application
    /// sessions.
    /// </summary>
    public interface IEntity
    {
    }

    /// <summary>
    ///     A entity that that can be persisted and can be identified by a single primary key.
    /// </summary>
    /// <typeparam name="TKey">The entity key type.</typeparam>
    public interface IEntity<out TKey> : IEntity
    {
        TKey Id { get; }
    }

    /// <summary>
    ///     A entity that can be persisted and can be identified by an assignable single primary key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public interface IEntityMutableKey<TKey> : IEntity<TKey>
    {
        new TKey Id { get; set; }
    }
}