#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace FutureState.Specifications
{
    /// <summary>
    /// Requires a non empty default guid value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class GuidRequiredAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if ((Guid)value == Guid.Empty)
            {
                return new ValidationResult("A valid not empty guid is required.");
            }

            return ValidationResult.Success;
        }
    }
}