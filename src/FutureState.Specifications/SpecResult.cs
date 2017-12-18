#region

using System;

#endregion

namespace FutureState.Specifications
{
    /// <summary>
    ///     A data structure used to communicate the result of a validation routine.
    /// </summary>
    public class SpecResult
    {
        /// <summary>
        ///     Passed validation.
        /// </summary>
        public static readonly SpecResult Success = new SpecResult(true, string.Empty);

        /// <summary>
        ///     Creates a new instance
        /// </summary>
        /// <param name="isValid">The validity state of the entity evaluated.</param>
        /// <param name="errorMessage">The error message.</param>
        public SpecResult(bool isValid, string errorMessage = @"")
        {
            if (!isValid)
                if (string.IsNullOrWhiteSpace(errorMessage))
                    throw new ArgumentOutOfRangeException(nameof(errorMessage),
                        "Message is empty but is valid is false.");

            IsValid = isValid;
            DetailedErrorMessage = errorMessage;
        }

        /// <summary>
        ///     Creates an invalid spec result.
        /// </summary>
        public SpecResult(string errorMessage)
            : this(false, errorMessage)
        {
            Guard.ArgumentNotNullOrEmpty(errorMessage, nameof(errorMessage));
        }

        /// <summary>
        ///     Creates a 'valid' spec result (validation result).
        /// </summary>
        public SpecResult()
        {
            IsValid = true;
            DetailedErrorMessage = string.Empty;
        }

        /// <summary>
        ///     Gets a more detailed description of why a given entity was, or was not, valid.
        /// </summary>
        public string DetailedErrorMessage { get; }

        /// <summary>
        ///     Gets whether the result of a specification evaluation is valid or not.
        /// </summary>
        public bool IsValid { get; }

        /// <summary>
        ///     Gets the validity sate of the spec result.
        /// </summary>
        /// <param name="result">The result of the specification valuation.</param>
        /// <returns></returns>
        public static implicit operator bool(SpecResult result)
        {
            return result.IsValid;
        }

        /// <summary>
        ///     Formats the object result.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return @"{0} - {1}".Params(IsValid ? @"Valid" : "InValid", DetailedErrorMessage);
        }
    }
}