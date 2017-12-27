#region

using System.Collections;
using System.ComponentModel.DataAnnotations;
using NLog;

#endregion

namespace FutureState.Specifications
{
    /// <summary>
    ///     Requires a populated collection of one or more elements.
    /// </summary>
    public sealed class NotEmptyCollectionValidationAttribute : ValidationAttribute
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly string _name;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotEmptyCollectionValidationAttribute" /> class.
        /// </summary>
        /// <param name="name">The name to used in error messages.</param>
        public NotEmptyCollectionValidationAttribute(string name)
        {
            _name = name;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            // validationContext.ObjectType
            if (validationContext != null && validationContext.ObjectType != null)
                if (validationContext.ObjectType.IsInstanceOfType(typeof(ICollection)))
                    if (Logger.IsDebugEnabled)
                        Logger.Debug("Instance of IList");

            if (Logger.IsDebugEnabled)
                Logger.Debug("Validating list object");

            var collection = value as ICollection;

            if (collection == null || collection.Count == 0)
            {
                var result =
                    new ValidationResult(ErrorMessage = $"{_name} cannot be an empty or null list.");

                if (Logger.IsDebugEnabled)
                    Logger.Debug("List {0} is empty and not valid.", collection);

                return result;
            }

            return ValidationResult.Success;
        }
    }
}