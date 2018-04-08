namespace FutureState.Data
{
    /// <summary>
    ///     Something that can insert something into an underlying data store.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to insert.</typeparam>
    public interface IInserter<in TEntity>
    {
        /// <summary>
        ///     Inserts a new entity into the data store.
        /// </summary>
        /// <param name="item">Required. The entity to insert.</param>
        /// <exception cref="ArgumentNullException">
        ///     Raised if entity is null.
        /// </exception>
        void Insert(TEntity item);
    }
}