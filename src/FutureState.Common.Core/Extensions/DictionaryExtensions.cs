#region

using System;
using System.Collections.Generic;
using System.Linq;
using FutureState.Data;

#endregion

namespace FutureState
{
    public static class DictionaryExtensions
    {
        /// <summary>
        ///     Gets a string value from a string dictionary object.
        /// </summary>
        public static string Get(this Dictionary<string, string> dictionary, string key)
        {
            if (dictionary.ContainsKey(key))
                return dictionary[key];

            return null;
        }

        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            dictionary.TryGetValue(key, out value);
            return value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<TValue> defaultValueProvider)
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value))
                return defaultValueProvider();

            return value;
        }

        public static bool IsEquivalentTo<TKey, TValue>(this IDictionary<TKey, TValue> lhs,
            IDictionary<TKey, TValue> rhs, IEqualityComparer<TValue> valueComparer = null)
        {
            if (lhs == rhs)
                return true;
            if (lhs == null || rhs == null)
                return false;
            if (lhs.Count != rhs.Count)
                return false;

            valueComparer = valueComparer ?? EqualityComparer<TValue>.Default;

            foreach (var kvp in lhs)
            {
                TValue secondValue;
                if (!rhs.TryGetValue(kvp.Key, out secondValue))
                    return false;
                if (!valueComparer.Equals(kvp.Value, secondValue))
                    return false;
            }
            return true;
        }

        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> left,
            IDictionary<TKey, TValue> right)
        {
            return left.Concat(right).ToDictionary(p => p.Key, p => p.Value);
        }

        /// <summary>
        ///     Adds or updates a string value to a given string dictionary.
        /// </summary>
        /// x
        public static void Set(this Dictionary<string, string> dictionary, string key, string value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentNullException(nameof(key), "Key cannot be a null or empty value.");

            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        public static void Set<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key), "Key cannot be a null or empty value.");

            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        public static BatchUpdateResult ToBatchUpdateResult<T>(this IEnumerable<UpdateResult<T>> list)
        {
            int added = 0,
                updated = 0,
                deleted = 0;

            list.Each(
                value =>
                {
                    switch (value.Result)
                    {
                        case UpdateResult.Added:
                            added++;
                            break;

                        case UpdateResult.Updated:
                            updated++;
                            break;

                        case UpdateResult.Deleted:
                            deleted++;
                            break;
                    }
                });

            return new BatchUpdateResult(added, updated, deleted);
        }

        /// <summary>
        ///     Builds a dictionary populated with unique keys based an an enumerable set of entities.
        /// </summary>
        public static Dictionary<TKey, TValue> ToUniqueDictionary<TKey, TValue>(
            this IEnumerable<TValue> values,
            Func<TValue, TKey> getKeyFunc)
        {
            return values.ToUniqueDictionary(getKeyFunc, v => v);
        }

        /// <summary>
        ///     Builds a dictionary with unique keys even if the key function produces a duplicate key.
        /// </summary>
        public static Dictionary<TKey, TValue> ToUniqueDictionary<TKey, TValue>(
            this IEnumerable<TValue> values,
            Func<TValue, TKey> getKeyFunc,
            Func<TValue, TValue> getValueFunc,
            Action<Exception> onDuplicateKeyDetected)
        {
            try
            {
                return values.ToUniqueDictionary(getKeyFunc, v => v);
            }
            catch (Exception ex)
            {
                onDuplicateKeyDetected?.Invoke(ex);

                //react to issue //assume this is duplicaet key and filter out
                return values.ToDictionary(getKeyFunc, getValueFunc);
            }
        }

        public static Dictionary<TKey, TValue> ToUniqueDictionary<TSource, TKey, TValue>(
            this IEnumerable<TSource> values,
            Func<TSource, TKey> getKeyFunc,
            Func<TSource, TValue> getValueFunc)
        {
            var dictionary = new Dictionary<TKey, TValue>();

            foreach (var item in values)
            {
                var key = getKeyFunc(item);

                if (!dictionary.ContainsKey(key))
                    dictionary.Add(key, getValueFunc(item));
            }

            return dictionary;
        }

        /// <summary>
        ///     Builds a dictionary populated with unique keys based an an enumerable set of entities.
        /// </summary>
        public static Dictionary<TKey, TValue> ToUniqueDictionary<TKey, TValue>(
            this IEnumerable<TValue> values,
            Func<TValue, TKey> getKeyFunc,
            IEqualityComparer<TKey> comparer)
        {
            var dictionary = new Dictionary<TKey, TValue>(comparer);

            foreach (var result in values)
            {
                var key = getKeyFunc(result);

                if (!dictionary.ContainsKey(key))
                    dictionary.Add(key, result);
            }

            return dictionary;
        }

        public static IEnumerable<TK> GetKeysSafe<TK, TV>(this Dictionary<TK, TV> dictionary)
        {
            if (dictionary == null) return Enumerable.Empty<TK>();
            return dictionary.Keys;
        }

        public static bool ContainsKeySafe<TK, TV>(this Dictionary<TK, TV> dictionary, TK key)
        {
            if (dictionary == null) return false;
            return dictionary.ContainsKey(key);
        }
    }
}