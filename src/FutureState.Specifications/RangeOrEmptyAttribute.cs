#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace FutureState.Specifications
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class RangeOrEmptyAttribute : RangeAttribute
    {
        public RangeOrEmptyAttribute(double minimum, double maximum) : base(minimum, maximum)
        {
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            var val = Convert.ToDouble(value);

            return base.IsValid(val, validationContext);
        }
    }
}