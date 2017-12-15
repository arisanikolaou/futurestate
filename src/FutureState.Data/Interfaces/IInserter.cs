using System;

namespace FutureState.Data
{
    // a.n.a this appears to be vapourware ... i can't see how this object would be used, ever, on its own.
    // this leads me to think that every interface should have at most one method or method overload which i don't think
    // is the objective of the interface segregation principle

    /// <summary>
    /// Something that can insert something into an underlying repository.
    /// </summary>
    /// <typeparam name="TEntity">The entity type to insert.</typeparam>
    public interface IInserter<in TEntity>
    {
        /// <summary>
        /// Inserts a new entity into the data store.
        /// </summary>
        /// <param name="item">Required. The entity to insert.</param>
        /// <exception cref="ArgumentNullException">
        /// Raised if entity is null.
        /// </exception>
        void Insert(TEntity item);
    }
}