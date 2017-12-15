#region

using NLog;
using System;
using System.Linq;
using System.Text;

#endregion

namespace FutureState.Specifications
{
    // this is a IValidator wrapper

    /// <summary>
    /// Validates a given entity or service against a given <see cref="IProvideSpecifications{TEntity}" /> instance.
    /// </summary>
    /// <remarks>
    /// Value types are not expected to be handled by this instance or null value.
    /// </remarks>
    /// <typeparam name="TSpecProvider">The type providing the rules.</typeparam>
    /// <typeparam name="TEntity">The class type of entity being validated.</typeparam>
    public class ValidateWithSpecs<TSpecProvider, TEntity> : IValidator
        where TSpecProvider : IProvideSpecifications<TEntity>, new()
        where TEntity : class
    {
        // ReSharper disable once StaticFieldInGenericType
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private static readonly ISpecification<TEntity>[] Specs;

        /// <summary>
        /// Builds a static list of specs for the given type once per application lifetime.
        /// </summary>
        static ValidateWithSpecs()
        {
            if (Logger.IsDebugEnabled)
            {
                Logger.Debug(
                    "Activating {0} to get list of specifications for entity or service {1}.",
                    typeof(TSpecProvider),
                    typeof(TEntity));
            }

            // activate only once when the assembly is loaded
            Specs = Activator.CreateInstance<TSpecProvider>().GetSpecifications().ToArray();
        }

        /// <summary>
        /// The error message to include in the details.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// The name of the field to include in error messages.
        /// </summary>
        public string Name { get; set; }

        // always maintain parameter less constructor

        /// <summary>
        /// Validates any not null value which derives from <see cref="TEntity" />.
        /// </summary>
        /// <param name="value">A null or instance object. Null objects will not be validated.</param>
        /// <returns>
        /// True if valid.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        /// Can't validate objects not assignable from
        /// {0}.Params(typeof(TEntity))
        /// </exception>
        public bool Validate(object value)
        {
            if (value == null)
            {
                // rely on required attribute to validate
                return true;
            }

            if (value is TEntity)
            {
                var sb = new StringBuilder();
                var isValid = true;

                foreach (var spec in Specs)
                {
                    var result = spec.Evaluate(value as TEntity);

                    if (!result.IsValid)
                    {
                        sb.Append(result.DetailedErrorMessage);
                        isValid = false;
                    }
                }

                if (!isValid)
                {
                    // faster than any
                    if (!string.IsNullOrWhiteSpace(Name))
                    {
                        ErrorMessage = "{0} has the following errors: \n {1}".Params(Name, sb);
                    }
                    else
                    {
                        ErrorMessage = sb.ToString();
                    }
                }

                return isValid;
            }

            throw new InvalidOperationException("Can't validate objects not assignable from {0}".Params(typeof(TEntity)));
        }
    }
}