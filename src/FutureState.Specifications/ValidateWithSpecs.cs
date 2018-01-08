#region

using System;
using System.Linq;
using System.Text;
using NLog;

#endregion

namespace FutureState.Specifications
{
    // an IValidator wrapper

    /// <summary>
    ///     Validates a given entity or service against a given <see cref="IProvideSpecifications{TEntity}" /> instance.
    /// </summary>
    /// <remarks>
    ///     Value types are not expected to be handled by this instance or null value.
    /// </remarks>
    /// <typeparam name="TSpecProvider">The type providing the rules.</typeparam>
    /// <typeparam name="TEntity">The class type of entity being validated.</typeparam>
    public class ValidateWithSpecs<TSpecProvider, TEntity> : IValidator
        where TSpecProvider : IProvideSpecifications<TEntity>, new()
        where TEntity : class
    {
        // ReSharper disable once StaticFieldInGenericType
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private static readonly ISpecification<TEntity>[] _specs;

        /// <summary>
        ///     Builds a static list of specs for the given type once per application lifetime.
        /// </summary>
        static ValidateWithSpecs()
        {
            if (_logger.IsDebugEnabled)
            {
                _logger.Debug(
                    "Activating {0} to get list of specifications for entity or service {1}.",
                    typeof(TSpecProvider).FullName,
                    typeof(TEntity).FullName);
            }

            // activate only once when the assembly is loaded
            _specs = Activator.CreateInstance<TSpecProvider>()
                .GetSpecifications().ToArray();
        }

        /// <summary>
        ///     The error message to include in the details.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        ///     The name of the field to include in error messages.
        /// </summary>
        public string Name { get; set; }

        // always maintain parameter less constructor

        /// <summary>
        ///     Validates any not null value which derives from <see cref="TEntity" />.
        /// </summary>
        /// <param name="subject">A null or instance object. Null objects will not be validated.</param>
        /// <returns>
        ///     True if valid.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">
        ///     Can't validate objects not assignable from
        ///     {0}.Params(typeof(TEntity))
        /// </exception>
        public bool Validate(object subject)
        {
            if (subject == null)
                return true;

            if (!(subject is TEntity))
                throw new InvalidOperationException(
                    $"Can't validate objects not assignable from {typeof(TEntity).Name}");

            var sb = new StringBuilder();
            var isValid = true;

            foreach (var spec in _specs)
            {
                var result = spec.Evaluate((TEntity) subject);

                if (result.IsValid)
                    continue;

                sb.Append(result.DetailedErrorMessage);

                isValid = false;
            }

            if (isValid)
                return true;

            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (!string.IsNullOrWhiteSpace(Name))
                ErrorMessage = $"{Name} has the following errors: \n {sb}";
            else
                ErrorMessage = sb.ToString();

            return false;
        }
    }
}