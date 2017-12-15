namespace FutureState.Data
{
    /// <summary>
    /// A entity that that can be persisted and can be identified by a single primary key.
    /// </summary>
    /// <typeparam name="TKey">The entity id type.</typeparam>
    public interface IEntity<out TKey>
    {
        TKey Id { get; }
    }

    /// <summary>
    /// A entity that can be persisted and can be identified by an assignable single primary key.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    public interface IEntityMutableKey<TKey> : IEntity<TKey>
    {
        new TKey Id { get; set; }
    }
}