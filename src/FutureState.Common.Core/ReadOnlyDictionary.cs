#region

using System;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace FutureState
{
    // take from http://stackoverflow.com/questions/678379/is-there-a-read-only-generic-dictionary-available-in-net
    public class ReadOnlyDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IDictionary<TKey, TValue> _dictionary;

        public ReadOnlyDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
        }

        public ReadOnlyDictionary(IDictionary<TKey, TValue> dictionary)
        {
            Guard.ArgumentNotNull(dictionary, nameof(dictionary));

            _dictionary = dictionary;
        }

        public TValue this[TKey key] => _dictionary[key];

        public int Count => _dictionary.Count;

        public bool IsReadOnly => true;

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get { return this[key]; }

            set { throw ReadOnlyException(); }
        }

        public ICollection<TKey> Keys => _dictionary.Keys;

        public ICollection<TValue> Values => _dictionary.Values;

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return _dictionary.Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            _dictionary.CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
        {
            throw ReadOnlyException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            throw ReadOnlyException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            throw ReadOnlyException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        bool IDictionary<TKey, TValue>.Remove(TKey key)
        {
            throw ReadOnlyException();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            throw ReadOnlyException();
        }

        private static NotSupportedException ReadOnlyException()
        {
            return new NotSupportedException("This dictionary is read-only");
        }
    }
}