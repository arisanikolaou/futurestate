using System;

namespace FutureState.Specifications
{
    //todo: unit test

    public class ValueMatchingSpecification<T> : ISpecification<T>
    {
        private readonly Func<string, T, object> _getValueForKey;

        private readonly Func<object, bool> _isSatisfied;

        public ValueMatchingSpecification(string key, Func<string, T, object> getValueForKey,
            Func<object, bool> isSatisfied)
        {
            if (getValueForKey == null)
                throw new ArgumentNullException(nameof(getValueForKey));

            if (isSatisfied == null)
                throw new ArgumentNullException(nameof(isSatisfied));

            _getValueForKey = getValueForKey;
            _isSatisfied = isSatisfied;
            Key = key;
        }

        /// <summary>
        ///     Gets a description of the specification.
        /// </summary>
        public string Description => "Value matching specification for key " + Key;

        /// <summary>
        ///     Gets a key or code to identifier the specification.
        /// </summary>
        public string Key { get; }

        /// <summary>
        ///     Gets whether the specification is met by the given entity.
        /// </summary>
        public SpecResult Evaluate(T entity)
        {
            var value = _getValueForKey(Key, entity);
            var satisfied = _isSatisfied(value);
            return new SpecResult(satisfied,
                satisfied ? string.Empty : $"{Key} did not pass specification");
        }

        /// <summary>
        ///     Returns a string that represents the current object.
        /// </summary>
        /// <returns>
        ///     A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            return _isSatisfied.Target.GetInstanceFieldValue("tpcRule", false).ToString();
        }
    }
}