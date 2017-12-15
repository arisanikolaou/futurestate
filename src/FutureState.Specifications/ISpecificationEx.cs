#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace FutureState.Specifications
{
    /// <summary>
    /// Extension methods for <see cref="ISpecification{T}" />
    /// </summary>
    public static class SpecificationEx
    {
        public static ISpecification<TEntity> And<TEntity>(this ISpecification<TEntity> s1, ISpecification<TEntity> s2)
        {
            return new AndSpecification<TEntity>(s1, s2);
        }

        public static ISpecification<TEntity> Not<TEntity>(this ISpecification<TEntity> s)
        {
            return new NotSpecification<TEntity>(s);
        }

        public static ISpecification<TEntity> Or<TEntity>(this ISpecification<TEntity> s1, ISpecification<TEntity> s2)
        {
            return new OrSpecification<TEntity>(s1, s2);
        }

        /// <summary>
        /// Will throw a <see cref="RuleException" /> if any errors are detected.
        /// </summary>
        /// <param name="validatable">The entity that is validatable.</param>
        /// <exception cref="RuleException">Raised if any errors are detected in the validatable instance.</exception>
        public static void ThrowExceptionIfInvalid(this IValidatable validatable)
        {
            Guard.ArgumentNotNull(validatable, nameof(validatable));

            var errors = validatable.Validate().ToArray();

            if (errors.Any())
            {
                var sb = new StringBuilder();
                foreach (var error in errors)
                {
                    sb.AppendLine(error.ToString());
                }
                var message = sb.ToString();

                throw new RuleException(message, errors);
            }
        }

        /// <summary>
        /// Will throw a <see cref="RuleException" /> if any errors are detected.
        /// </summary>
        /// <param name="validatable">The entity that is validatable.</param>
        /// <param name="getMessage">The function to construct the error message.</param>
        /// <exception cref="RuleException">Raised if any errors are detected in the validatable instance.</exception>
        public static void ThrowExceptionIfInvalid(this IValidatable validatable, Func<string> getMessage)
        {
            Guard.ArgumentNotNull(validatable, nameof(validatable));
            Guard.ArgumentNotNull(getMessage, nameof(getMessage));

            var errors = validatable.Validate().ToArray();
            if (errors.Any())
            {
                throw new RuleException(getMessage?.Invoke(), errors);
            }
        }

        public static IEnumerable<ISpecification<TEntity>> GetInvalid<TEntity>(
            this IEnumerable<ISpecification<TEntity>> specs,
            TEntity entity)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var spec in specs)
            {
                if (!spec.Evaluate(entity).IsValid)
                {
                    yield return spec;
                }
            }
        }

        public static IEnumerable<Error> ToErrors<TEntity>(
            this IEnumerable<ISpecification<TEntity>> specs,
            TEntity entity,
            string category = null)
        {
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach (var spec in specs)
            {
                var result = spec.Evaluate(entity);

                if (!result.IsValid)
                {
                    yield return new Error(result.DetailedErrorMessage, spec.Key, category);
                }
            }
        }

        /// <summary>
        /// Raises a <see cref="RuleException" /> if the entity tested has errors.
        /// </summary>
        /// <typeparam name="TEntity">The entity type to test.</typeparam>
        /// <param name="specs">The specs to use to test the entity.</param>
        /// <param name="entity">The entity to test.</param>
        /// <param name="exceptionMessageText">The 'headerr' exception text to display.</param>
        /// <param name="category"></param>
        public static void ThrowExceptionIfInvalid<TEntity>(
            this IEnumerable<ISpecification<TEntity>> specs,
            TEntity entity,
            string exceptionMessageText,
            string category = null)
        {
            var errors = ToErrors(specs, entity, category);

            if (errors.Any())
            {
                throw new RuleException(exceptionMessageText, errors);
            }
        }

        /// <summary>
        /// Raises a <see cref="RuleException" /> if the entity tested has errors.
        /// </summary>
        public static void ThrowExceptionIfInvalid<TEntity>(
            this IProvideSpecifications<TEntity> specProvider,
            TEntity entity,
            string exceptionMessageText,
            string category = null)
        {
            ThrowExceptionIfInvalid(specProvider.GetSpecifications(), entity, category);
        }

        /// <summary>
        /// Finds the first specification matching a given key.
        /// </summary>
        public static ISpecification<TEntity> Find<TEntity>(this IEnumerable<ISpecification<TEntity>> specs, string key)
        {
            return specs.FirstOrDefault(m => string.Equals(m.Key, key, StringComparison.OrdinalIgnoreCase));
        }
    }
}