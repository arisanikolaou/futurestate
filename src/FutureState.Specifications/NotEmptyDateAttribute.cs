﻿#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace FutureState.Specifications
{
    /// <summary>
    ///     Used to ensure that a date value is not a default date value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
    public sealed class NotEmptyDateAttribute : ValidationAttribute
    {
        private readonly string _fieldName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NotEmptyDateAttribute" /> class.
        /// </summary>
        public NotEmptyDateAttribute(string fieldName)
        {
            _fieldName = fieldName;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is DateTime)
                if ((DateTime)value == default(DateTime))
                    return new ValidationResult($"'{_fieldName}' must not be a default date time value.");

            // else
            // throw new InvalidOperationException("The type of object '{0}' is not a DateTime type.".Params(value.GetType().FullName));
            return ValidationResult.Success;
        }
    }
}