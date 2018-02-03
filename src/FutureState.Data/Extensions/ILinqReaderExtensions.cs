#region

using System;
using System.Linq.Expressions;

#endregion

namespace FutureState.Data
{
    /// <summary>
    ///     Handy extensions to enhance linq reader based queries.
    /// </summary>
    public static class ILinqReaderExtensions
    {
        /// <summary>
        ///     Gets the first element from the reader in the predicate.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to return.</typeparam>
        /// <typeparam name="TKey">The entity key type.</typeparam>
        /// <param name="reader"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static TEntity First<TEntity, TKey>(this ILinqReader<TEntity, TKey> reader,
            Expression<Func<TEntity, bool>> predicate)
            where TEntity : class
        {
            Guard.ArgumentNotNull(reader, nameof(reader));
            Guard.ArgumentNotNull(predicate, nameof(predicate));

            var item = reader.FirstOrDefault(predicate);

            if (item == null)
                throw new InvalidOperationException("No element satisfies the condition in predicate.");

            return item;
        }

        /// <summary>
        ///     Gets the first element in a given reader.
        /// </summary>
        public static T First<T, K>(this ILinqReader<T, K> reader)
            where T : class
        {
            Guard.ArgumentNotNull(reader, nameof(reader));

            return reader.First(_ => true);
        }

        /// <summary>
        ///     Gets a single result from a linq reader.
        /// </summary>
        public static TEntity Single<TEntity, TKey>(this ILinqReader<TEntity, TKey> reader,
            Expression<Func<TEntity, bool>> predicate)
            where TEntity : class
        {
            Guard.ArgumentNotNull(reader, nameof(reader));
            Guard.ArgumentNotNull(predicate, nameof(predicate));

            var item = reader.SingleOrDefault(predicate);
            if (item == null)
                throw new InvalidOperationException("No element satisfies the condition in predicate.");

            return item;
        }

        /// <summary>
        ///     Gets a single result from the reader.
        /// </summary>
        public static TEntity Single<TEntity, TKey>(this ILinqReader<TEntity, TKey> reader)
            where TEntity : class
        {
            Guard.ArgumentNotNull(reader, nameof(reader));

            return reader.Single(_ => true);
        }
    }
}