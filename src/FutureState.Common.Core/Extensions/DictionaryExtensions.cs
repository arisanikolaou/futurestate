#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace FutureState
{
    public static class DictionaryExtensions
    {
        public static TValue Get<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.TryGetValue(key, out var value);

            return value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.TryGetValue(key, out var value);

            return value;
        }

        public static TValue GetValueOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key,
            Func<TValue> defaultValueProvider)
        {
            if (!dictionary.TryGetValue(key, out var value))
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
                if (!rhs.TryGetValue(kvp.Key, out var secondValue))
                    return false;
                if (!valueComparer.Equals(kvp.Value, secondValue))
                    return false;
            }

            return true;
        }

        public static IDictionary<TKey, TValue> Merge<TKey, TValue>(this IDictionary<TKey, TValue> left,
            IDictionary<TKey, TValue> right)
        {
            return left.Concat(right).ToUniqueDictionary(p => p.Key, p => p.Value);
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

        /// <summary>
        ///     Creates a dictionary composed of unique key pair values selected from a source list.
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="values"></param>
        /// <param name="getKeyFunc"></param>
        /// <param name="getValueFunc"></param>
        /// <returns></returns>
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
    }
}