#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

#endregion

namespace FutureState
{
    //todo: unit tests

    public static class EnumerableExtensions
    {
        /// <summary>
        ///     Compares two string arrays by length, order but not case sensitivity.
        /// </summary>
        public static bool AreEquivalent(this string[] namesA, string[] namesB)
        {
            Guard.ArgumentNotNull(namesA, nameof(namesA));
            Guard.ArgumentNotNull(namesB, nameof(namesB));

            if (namesA.Length != namesB.Length)
                return false;

            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < namesA.Length; i++)
                if (!namesA[i].Equals(namesB[i], StringComparison.OrdinalIgnoreCase))
                    return false;

            return true;
        }

        public static bool IsEquivalentTo<T>(this IEnumerable<T> source, IEnumerable<T> other,
            Func<T, T, bool> equals = null)
        {
            if (Equals(source, other))
                return true;
            if (source == null || other == null)
                return false;

            var lhsList = source.ToList();
            var rhsList = other.ToList();

            if (lhsList.Count != rhsList.Count)
                return false;

            if (equals == null)
                equals = (a, b) => Equals(a, b);

            for (var i = 0; i < lhsList.Count; i++)
                if (!equals(lhsList[i], rhsList[i]))
                    return false;
            return true;
        }


        /// <summary>
        ///     Yields the union of a and b.
        /// </summary>
        public static IEnumerable<TItem> Combine<TItem>(this IEnumerable<TItem> a, IEnumerable<TItem> b)
        {
            foreach (var item in a)
                yield return item;

            foreach (var item in b)
                yield return item;
        }

        // faster the executing immediate by about 20 % via FirstOrDefault or returning an Array

        /// <summary>
        ///     Executes a deferred query yielding an enumeration immediately to the caller.
        /// </summary>
        public static IEnumerable<T> ExecImmediate<T>(this IEnumerable<T> enumerable)
        {
            return enumerable;
        }

        /// <summary>
        ///     Create a batch of certain size.
        /// </summary>
        /// <param name="items">Enumerable to check.</param>
        /// <param name="maxItems">Size for the batch.</param>
        /// <returns></returns>
        public static IEnumerable<IEnumerable<T>> BatchEx<T>(this IEnumerable<T> items, int maxItems)
        {
            return items?.Select((item, inx) => new {item, inx})
                .GroupBy(x => x.inx / maxItems)
                .Select(g => g.Select(x => x.item));
        }

        /// <summary>
        ///     Combines to enumerations into an aggregate enumeration and treats null enumerations as zero length arrays.
        /// </summary>
        public static IEnumerable<T> ConcatEx<T>(this IEnumerable<T> enumOne, IEnumerable<T> enumTwo)
        {
            if (enumOne == null)
                return enumTwo;

            if (enumTwo == null)
                return enumOne;

            return enumOne.Concat(enumTwo);
        }

        public static bool ContainsAny<T>(this IEnumerable<T> source, IEnumerable<T> targets)
        {
            var enumerable = targets as T[] ?? targets.ToArray();

            if (!enumerable.Any())
                return true;

            var hash = source.ToHashSet();

            return enumerable.Any(hash.Contains);
        }

        /// <summary>
        ///     Used to flatten dictionary to the string.
        /// </summary>
        /// <typeparam name="TKey">Dictionary key type.</typeparam>
        /// <typeparam name="TValue">Dictionary value type.</typeparam>
        /// <param name="dictionary"></param>
        /// <param name="keyValueSeparator">Symbol to use as separator between key and value in the result string.</param>
        /// <param name="sequenceSeparator">Symbol to use between the pairs in the result string.</param>
        /// <param name="take">Specified number of elements to take from the dictionary</param>
        /// <returns>
        ///     <c>String that represents the dictionary.</c>
        /// </returns>
        public static string DictionaryToString<TKey, TValue>(
            this IDictionary<TKey, TValue> dictionary,
            string keyValueSeparator,
            string sequenceSeparator,
            int take = int.MaxValue)
        {
            if (dictionary == null)
                return null;

            var stringBuilder = new StringBuilder();
            dictionary.Take(take)
                .Each(
                    x =>
                        stringBuilder.AppendFormat("{0}{1}{2}{3}", x.Key.ToString(), keyValueSeparator,
                            x.Value.ToString(), sequenceSeparator));

            return stringBuilder.ToString(0, stringBuilder.Length - sequenceSeparator.Length);
        }

        /// <summary>
        ///     Gets a hash usign hashes of individual elements
        ///     NOTE this is not GetHashCode override since we should try to use immutable elements when computing hashcodes
        ///     This is for purposes of detecting equal(in terms of elements equality) collections
        ///     Does not throw an exception on null
        ///     Null elements are ignored thus two collections that differ only by number of nulls will return the same result
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="firstCollection"></param>
        /// <returns></returns>
        public static int GetEnumerableHash<T>(this IEnumerable<T> firstCollection)
        {
            if (firstCollection == null)
                return 0;

            // we store this since we want to use nonNullElemsHashCodes.Count as initial seed to minimize collision of hashes
            // e.g. without it two lists {0,0,1} and {0,1} would produce the same results
            var nonNullElemsHashCodes =
                firstCollection.Where(elem => elem != null).Select(elem => elem.GetHashCode()).ToList();

            return
                nonNullElemsHashCodes.OrderBy(hash => hash)
                    .Aggregate(nonNullElemsHashCodes.Count, (res, i) => (res * 397) ^ i);
        }

        /// <summary>
        ///     Similar to previous one, but allows to pass a hashfunction
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="firstCollection"></param>
        /// <param name="hashFunc"></param>
        /// <returns></returns>
        public static int GetEnumerableHash<T>(this IEnumerable<T> firstCollection, Func<T, int> hashFunc)
        {
            if (firstCollection == null)
                return 0;

            // we store this since we want to use nonNullElemsHashCodes.Count as initial seed to minimize collision of hashes
            // e.g. without it two lists {0,0,1} and {0,1} would produce the same results
            var nonNullElemsHashCodes = firstCollection.Where(elem => elem != null).Select(hashFunc).ToList();

            return
                nonNullElemsHashCodes.OrderBy(hash => hash)
                    .Aggregate(nonNullElemsHashCodes.Count, (res, i) => (res * 397) ^ i);
        }

        public static bool IsAnyContainedIn<T>(this IEnumerable<T> targets, params T[] source)
        {
            if (targets == null || !targets.Any())
                return true;

            if (source == null)
                return false;

            var hash = source.ToHashSet();

            return targets.Any(hash.Contains);
        }

        /// <summary>
        ///     Determines whether provided collection is null or empty.
        /// </summary>
        /// <param name="collection">The collection to check.</param>
        /// <returns><c>True</c> if input is a non-null/non-empty collection. <c>False</c> otherwise.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNotNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection != null && collection.Any();
        }

        /// <summary>
        ///     Determines whether provided collection is null or empty.
        /// </summary>
        /// <param name="collection">The collection to check.</param>
        /// <returns><c>True</c> if input collection is null or empty. <c>False</c> otherwise.</returns>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> collection)
        {
            return collection == null || !collection.Any();
        }

        /// <summary>
        ///     Wrist friendly call to IsNullOrWhiteSpace.
        /// </summary>
        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }

        public static IEnumerable<T> OfGenericType<T>(this IEnumerable<T> collection, Type genericType)
        {
            if (collection == null)
                yield break;

            foreach (var elem in collection)
            {
                var tp = elem.GetType();
                if (tp.IsGenericType && tp.GetGenericTypeDefinition() == genericType)
                    yield return elem;
            }
        }

        public static void Recursively<T>(this IEnumerable<T> collection, Func<T, IEnumerable<T>> accessor,
            Func<T, bool> stopCondition, Action<T> action)
        {
            foreach (var item in collection)
            {
                if (stopCondition(item))
                    continue;

                action?.Invoke(item);

                Recursively(accessor?.Invoke(item), accessor, stopCondition, action);
            }
        }

        /// <summary>
        ///     Slices a sequence into a sub-sequences each containing maxItemsPerSlice, except for the last
        ///     which will contain any items left over
        /// </summary>
        public static IEnumerable<IEnumerable<T>> Slice<T>(this IEnumerable<T> sequence, int maxItemsPerSlice)
        {
            if (maxItemsPerSlice <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxItemsPerSlice),
                    "maxItemsPerSlice must be greater than 0");

            var slice = new List<T>(maxItemsPerSlice);

            foreach (var item in sequence)
            {
                slice.Add(item);

                if (slice.Count == maxItemsPerSlice)
                {
                    yield return slice.ToArray();
                    slice.Clear();
                }
            }

            // return the "crumbs" (leftovers) that
            // didn't make it into a full slice
            if (slice.Count > 0)
                yield return slice.ToArray();
        }

        /// <summary>
        ///     Converts a non null enumeration to a collection
        /// </summary>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICollection<T> ToCollection<T>(this IEnumerable<T> items)
        {
            Guard.ArgumentNotNull(items, nameof(items));

            return items as ICollection<T> ?? items.ToList();
        }

        /// <summary>
        ///     Converts a non null enumeration to a collection
        /// </summary>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ICollection<T> ToCollectionSafe<T>(this IEnumerable<T> items)
        {
            if (items == null)
                return null;

            return items as ICollection<T> ?? items.ToList();
        }

        /// <summary>
        ///     Same as ToHashSet but returns null if items are null.
        /// </summary>
        [DebuggerStepThrough]
        public static HashSet<T> ToHashSetSafe<T>(this IEnumerable<T> items)
        {
            return items?.ToHashSet();
        }

        /// <summary>
        ///     Returns null if the array is null otherwise the array.
        /// </summary>
        [DebuggerStepThrough]
        public static T[] ToArraySafe<T>(this IEnumerable<T> items)
        {
            return items?.ToArray();
        }

        /// <summary>
        ///     This is similar to the standard Union.
        ///     Does not throw an exception if any collection is null
        /// </summary>
        public static IEnumerable<T> UnionEx<T>(this IEnumerable<T> firstCollection, IEnumerable<T> secondCollection)
        {
            if (firstCollection == null)
                return secondCollection;

            if (secondCollection == null)
                return firstCollection;

            return firstCollection.Union(secondCollection);
        }

        //todo: replace by more linq?
        /// <summary>
        ///     Gets a list of unique elements by a given key.
        /// </summary>
        /// <remarks>
        ///     Taken from here:
        ///     http://stackoverflow.com/questions/489258/linq-distinct-on-a-particular-property
        /// </remarks>
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var element in source)
                if (seenKeys.Add(keySelector(element)))
                    yield return element;
        }

        /// <summary>
        ///     Executes a callback/action method against every element of a sequence. If the sequence is null then null is
        ///     returned to the caller.
        /// </summary>
        [DebuggerStepThrough]
        public static IEnumerable<T> Each<T>(this IEnumerable<T> sequence, Action<T> callback)
        {
            if (callback == null || sequence == null)
                return sequence;

            foreach (var obj in sequence)
                callback?.Invoke(obj);

            //return the incoming list, array
            return sequence;
        }

        /// <summary>
        ///     Creates an enumeration from a single element.
        /// </summary>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ToEnumerable<T>(this T @object)
        {
            yield return @object;
        }

        /// <summary>
        ///     Creates an enumeration from a single element. Returns an empty enumerable if object is null
        /// </summary>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<T> ToEnumerableExNull<T>(this T @object)
        {
            if (@object == null)
                yield break;
            yield return @object;
        }

        /// <summary>
        ///     Converts a sequence to a hash set.
        /// </summary>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> collection)
        {
            return collection as HashSet<T> ?? new HashSet<T>(collection);
        }

        /// <summary>
        ///     Gets a concatenated string with the 'stringified' representation of each item in a sequence.
        /// </summary>
        public static string CollectionLog<T>(this IEnumerable<T> sequence) where T : class
        {
            var log = sequence == null
                ? "null"
                : string.Join(
                    ",",
                    sequence.Select(
                        (x, i) =>
                            $"[{i}]={(x == null ? "null" : x.ToString())}"));

            return log;
        }
    }
}