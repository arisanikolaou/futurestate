#region

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

#endregion

namespace FutureState.Specifications
{
    /// <summary>
    ///     Generic abstract base class to accumulate rules/specs for a given entity or service.
    /// </summary>
    /// <remarks>
    ///     Will automatically accumulate all specifications from an entity.
    /// </remarks>
    /// <typeparam name="TEntityOrService">
    ///     The entity or service to validate.
    /// </typeparam>
    public class SpecProvider<TEntityOrService> : IProvideSpecifications<TEntityOrService>
    {
        protected readonly ConcurrentBag<ISpecification<TEntityOrService>> _specs;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SpecProvider{TEntityOrService}" /> class.
        /// </summary>
        public SpecProvider()
        {
            _specs = new ConcurrentBag<ISpecification<TEntityOrService>>();

            foreach (var spec in new DataAnnotationsSpecProvider<TEntityOrService>().GetSpecifications())
                _specs.Add(spec);
        }

        /// <summary>
        ///     Gets the current list of specifications aggregated into the current instance.
        /// </summary>
        /// <returns>
        ///     List of one or more specifications.
        /// </returns>
        public IEnumerable<ISpecification<TEntityOrService>> GetSpecifications()
        {
            // discover any addition rules
            var evt = ResolveSpecifications;
            if (evt != null)
            {
                var args = new SpecEventArgs<TEntityOrService>();

                evt(this, args);

                // only add
                foreach (var spec in args.Specs)
                    _specs.Add(spec);
            }

            // return results
            return _specs.ToArray();
        }

        /// <summary>
        ///     Raised to discover specifications.
        /// </summary>
        public static event EventHandler<SpecEventArgs<TEntityOrService>> ResolveSpecifications;

        /// <summary>
        /// </summary>
        /// <param name="specAction"></param>
        /// <param name="key">The unique rule id.</param>
        /// <param name="specDescription">An optional default specDescription.</param>
        public SpecProvider<TEntityOrService> Add(
            Func<TEntityOrService, SpecResult> specAction,
            string key,
            string specDescription = "")
        {
            // will be added
            _specs.Add(new Specification<TEntityOrService>(specAction, key, specDescription)); // specDescription

            return this;
        }

        /// <summary>
        ///     Aggregates the specifications/rules from another spec provider into the current instance.
        /// </summary>
        /// <param name="specProvider">
        ///     The other specification provider for the entity or service.
        /// </param>
        public SpecProvider<TEntityOrService> Add(IProvideSpecifications<TEntityOrService> specProvider)
        {
            Guard.ArgumentNotNull(specProvider, nameof(specProvider));

            foreach (var spec in specProvider.GetSpecifications())
                _specs.Add(spec);

            return this;
        }

        /// <summary>
        ///     Adds a specification/rule using a given expression.
        /// </summary>
        /// <param name="condition">The expression to test that must be satisfied.</param>
        /// <param name="detailedMessage">The error message to display to the caller (user or system)</param>
        /// <param name="key">The field key.</param>
        /// <param name="specDescription">An optional field to describe the specification.</param>
        public SpecProvider<TEntityOrService> Add(
            Expression<Func<TEntityOrService, bool>> condition,
            Func<TEntityOrService, string> detailedMessage,
            string key,
            string specDescription = "")
        {
            Guard.ArgumentNotNull(condition, nameof(condition));
            Guard.ArgumentNotNull(detailedMessage, nameof(detailedMessage));
            Guard.ArgumentNotNullOrEmpty(key, nameof(key));

            //compile to method inline
            var specValidator = condition.Compile();

            var specification = new Specification<TEntityOrService>(
                m =>
                {
                    // ReSharper disable once ConvertIfStatementToReturnStatement
                    if (!specValidator.Invoke(m))
                        return new SpecResult(detailedMessage?.Invoke(m));

                    return SpecResult.Success;
                },
                key,
                specDescription);

            _specs.Add(specification);

            return this;
        }

        /// <summary>
        ///     Adds a specification/rule using a given expression.
        /// </summary>
        /// <param name="condition">The expression to test that must be satisfied.</param>
        /// <param name="key">The field key.</param>
        /// <param name="specDescription">An optional field to describe the specification.</param>
        public SpecProvider<TEntityOrService> Add(
            Expression<Func<TEntityOrService, bool>> condition,
            string key,
            string specDescription)
        {
            Guard.ArgumentNotNull(condition, nameof(condition));
            Guard.ArgumentNotNullOrEmpty(key, nameof(key));
            Guard.ArgumentNotNullOrEmpty(specDescription, nameof(specDescription));

            _specs.Add(
                new Specification<TEntityOrService>(
                    m =>
                    {
                        if (!condition.Compile().Invoke(m))
                            return new SpecResult(specDescription);

                        return SpecResult.Success;
                    },
                    key,
                    specDescription));

            return this;
        }
    }
}