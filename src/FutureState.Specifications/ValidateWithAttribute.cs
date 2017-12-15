#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace FutureState.Specifications
{
    /// <summary>
    /// Delegates validation of a given object to a type implementing <see cref="IValidator" />.
    /// </summary>
    public sealed class ValidateWithAttribute : ValidationAttribute
    {
        // don't inherit from required
        private readonly IValidator _validator;

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidateWithAttribute" /> class.
        /// </summary>
        /// <param name="validatorType">Type of the validator.</param>
        /// <param name="name">The name of the field to use in error message.</param>
        /// <exception cref="System.InvalidOperationException">
        /// The type {0} is not an implementation of
        /// {1}.Params(validatorType.FullName, typeof(IValidatable).FullName)
        /// </exception>
        public ValidateWithAttribute(Type validatorType, string name)
        {
            Guard.ArgumentNotNull(validatorType, nameof(validatorType));
            Guard.ArgumentNotNullOrEmpty(name, nameof(name));

            _validator = Activator.CreateInstance(validatorType) as IValidator;
            if (_validator == null)
            {
                throw new InvalidOperationException(
                    "The type {0} is not an implementation of {1}".Params(
                        validatorType.FullName,
                        typeof(IValidatable).FullName));
            }

            _validator.Name = name;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (!_validator.Validate(value))
            {
                ErrorMessage = _validator.ErrorMessage;

                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}