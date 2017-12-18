#region

using System;
using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

#endregion

namespace FutureState.Specifications
{
    /// <summary>
    ///     Used to validate complex structures against their declared data annotations.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ValidateWithAnnotationsAttribute : ValidationAttribute
    {
        private static readonly ConcurrentDictionary<Type, ISpecification<object>[]> SpecsCache =
            new ConcurrentDictionary<Type, ISpecification<object>[]>();

        private readonly string _fieldName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValidateWithAnnotationsAttribute" /> class.
        /// </summary>
        /// <param name="fieldName">The field name to include in error messages.</param>
        public ValidateWithAnnotationsAttribute(string fieldName)
        {
            _fieldName = fieldName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success; // can't validate rely on required attribute to ensure not null entity

            // should look up by dictionary
            var type = value.GetType();
            ISpecification<object>[] specs;

            // lazy load specs into internal cache
            if (SpecsCache.ContainsKey(type))
            {
                specs = SpecsCache[type];
            }
            else
            {
                var validator = new DataAnnotationsSpecProvider(type);

                specs = validator.GetSpecifications().ToArray();
                SpecsCache.TryAdd(type, specs);
            }

            var errors = specs.ToErrors(value);

            if (errors.Any())
            {
                var sb = new StringBuilder();
                sb.AppendFormat("{0}:", _fieldName);

                foreach (var error in errors)
                    sb.AppendFormat("  {0}\n", error.Message);

                ErrorMessage = sb.ToString();

                return new ValidationResult(ErrorMessage);
            }

            return ValidationResult.Success;
        }
    }
}