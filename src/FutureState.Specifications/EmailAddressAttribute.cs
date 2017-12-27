#region

using System;
using System.ComponentModel.DataAnnotations;

#endregion

namespace FutureState.Specifications
{
    // todo: change to e-mail address validation attribute
    /// <summary>
    ///     Requires a string value to translate to an e-mail address.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class EmailAddressAttribute : RegularExpressionAttribute
    {
        private const string pattern = @"^\w+([-+.]*[\w-]+)*@(\w+([-.]?\w+)){1,}\.\w{2,4}$";

        public EmailAddressAttribute()
            : base(pattern)
        {
        }
    }
}