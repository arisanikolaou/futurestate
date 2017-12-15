#region

using System.Collections.Generic;
using System.Linq;
using System.Text;

#endregion

namespace FutureState
{
    /// <summary>
    /// Extension methods for a given error or set of error objects.
    /// </summary>
    public static class ErrorExt
    {
        /// <summary>
        /// Exact search ofr an error with a type.
        /// </summary>
        public static Error Find(this IEnumerable<Error> errors, string type)
        {
            Guard.ArgumentNotNull(errors, nameof(errors));

            foreach (var error in errors)
            {
                if (error.Type == type)
                {
                    return error;
                }
            }

            return null;
        }

        /// <summary>
        /// Throw a <see cref="RuleException" /> if the errors enumeration is greater than zero with the given error
        /// message.
        /// </summary>
        public static void ThrowIfExists(this IEnumerable<Error> errors,
            string errorMessage = "One or more rules has been violated.")
        {
            Guard.ArgumentNotNull(errors, nameof(errors));

            var errorsArray = errors as Error[] ?? errors.ToArray();
            if (errorsArray.Any())
                throw new RuleException(errorMessage, errorsArray);
        }

        /// <summary>
        /// Converts a list of errors to a concatenated string by its 'message' property.
        /// </summary>
        public static string ToListString(this IEnumerable<Error> errors)
        {
            Guard.ArgumentNotNull(errors, nameof(errors));

            var sb = new StringBuilder();

            foreach (var eror in errors)
            {
                sb.AppendLine(eror.Message);
            }

            return sb.ToString();
        }
    }
}