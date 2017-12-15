#region

using System.Linq;

#endregion

namespace FutureState.Specifications
{
    public static class DataAnnotationsSpecProviderEx
    {
        /// <summary>
        /// Validates an object using a new <see cref="DataAnnotationsSpecProvider{T}" /> instance.
        /// </summary>
        public static void Validate<T>(T instance)
        {
            Guard.ArgumentNotNull(instance, nameof(instance));

            var specProvider = new DataAnnotationsSpecProvider<T>();
            var specs = specProvider.GetSpecifications();

            var errors = specs.ToErrors(instance);

            if (errors.Any())
            {
                throw new RuleException(@"Object violates specification.", errors);
            }
        }

        /// <summary>
        /// Validates an object using a given <see cref="DataAnnotationsSpecProvider{T}" /> instance.
        /// </summary>
        public static void Validate<T>(this DataAnnotationsSpecProvider<T> specProvider, T instance)
        {
            Guard.ArgumentNotNull(specProvider, nameof(specProvider));
            Guard.ArgumentNotNull(instance, nameof(instance));

            var specs = specProvider.GetSpecifications();

            var errors = specs.ToErrors(instance);

            if (errors.Any())
            {
                throw new RuleException(@"Object violates specification.", errors);
            }
        }
    }
}