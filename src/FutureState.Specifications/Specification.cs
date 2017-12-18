#region

using System;

#endregion

namespace FutureState.Specifications
{
    /// <summary>
    ///     A generic specification (or rule) for a given entity or service.
    /// </summary>
    /// <typeparam name="TEntity">The entity to evaluate the specification for.</typeparam>
    public sealed class Specification<TEntity> : ISpecification<TEntity>
    {
        private readonly Func<TEntity, SpecResult> _specAction;

        /// <summary>
        ///     Initializes a new instance of the <see cref="Specification{TEntity}" /> class.
        /// </summary>
        /// <param name="key">
        ///     The unique key for the given rule.
        /// </param>
        /// <param name="description">
        ///     The default description for the specification e.g. Name cannot be greater than 50 characters in length;
        /// </param>
        /// <param name="specAction">
        ///     The action to use to provide a <see cref="SpecResult" />
        /// </param>
        public Specification(Func<TEntity, SpecResult> specAction, string key, string description = "")
        {
            Guard.ArgumentNotNull(specAction, nameof(specAction));
            Guard.ArgumentNotNullOrEmpty(key, nameof(key));

            _specAction = specAction;
            Key = key; // consider string interning
            Description = description;
        }

        /// <summary>
        ///     Gets the description of the rule. E.g. age must be greater than zero. This can also be considered the error
        ///     message.
        /// </summary>
        public string Description { get; }

        /// <summary>
        ///     Gets the identifier for the rule.
        /// </summary>
        public string Key { get; }

        /// <summary>
        ///     Tests a given entity against the current instance and returns a result.
        /// </summary>
        public SpecResult Evaluate(TEntity entity)
        {
            return _specAction(entity);
        }

        /// <summary>
        ///     Gets the key and the description of the specification.
        /// </summary>
        public override string ToString()
        {
            return $@"{Key}: {Description}";
        }
    }
}