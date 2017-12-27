#region

using System.Collections.Generic;
using System.Linq;

#endregion

namespace FutureState.Specifications
{
    //todo: unit test

    /// <summary>
    ///     Validates a given entity or service of type T based on a set of rules derived from one or more specification
    ///     providers (rule books).
    /// </summary>
    /// <typeparam name="T">The type to validate.</typeparam>
    public class ValidatorAggregate<T> : IValidatorAggregate
    {
        private readonly List<ISpecification<T>> _aggregateSpecs;

        public ValidatorAggregate(IEnumerable<IProvideSpecifications<T>> specProviders)
        {
            Guard.ArgumentNotNull(specProviders, nameof(specProviders));

            _aggregateSpecs = new List<ISpecification<T>>();

            //an - will be duplictes if autofac registered
            var distinctSpecProviders = specProviders.DistinctBy(m => m.GetType()).ToList();

            foreach (var spec in distinctSpecProviders)
                _aggregateSpecs.AddRange(spec.GetSpecifications());
        }

        public IEnumerable<Error> GetErrors(object subject)
        {
            return _aggregateSpecs.ToErrors((T) subject);
        }

        public IEnumerable<Error> GetErrors(T subject)
        {
            return _aggregateSpecs.ToErrors(subject);
        }
    }
}