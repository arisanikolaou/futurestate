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
        public static T First<T, K>(this ILinqReader<T, K> reader, Expression<Func<T, bool>> predicate)
            where T : class
        {
            Guard.ArgumentNotNull(reader, nameof(reader));
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            var item = reader.FirstOrDefault(predicate);

            if (item == null)
                throw new InvalidOperationException("No element satisfies the condition in predicate.");

            return item;
        }

        public static T First<T, K>(this ILinqReader<T, K> reader)
            where T : class
        {
            Guard.ArgumentNotNull(reader, nameof(reader));

            return reader.First(_ => true);
        }

        public static T Single<T, K>(this ILinqReader<T, K> reader, Expression<Func<T, bool>> predicate)
            where T : class
        {
            Guard.ArgumentNotNull(reader, nameof(reader));
            Guard.ArgumentNotNull(predicate, nameof(predicate));
            var item = reader.SingleOrDefault(predicate);
            if (item == null)
                throw new InvalidOperationException("No element satisfies the condition in predicate.");

            return item;
        }

        public static T Single<T, K>(this ILinqReader<T, K> reader)
            where T : class
        {
            Guard.ArgumentNotNull(reader, nameof(reader));
            return reader.Single(_ => true);
        }
    }
}