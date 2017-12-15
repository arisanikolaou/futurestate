#region

using System.Collections.Generic;

#endregion

namespace FutureState.Specifications
{
    public static class SpecResultEx
    {
        /// <summary>
        /// Converts any invalid <see cref="SpecResult" /> into their equivalent 'error' objects.
        /// </summary>
        /// <param name="specResults">Required. The spec results to convert.</param>
        /// <param name="category">The error category to assign the results to when converted.</param>
        /// <returns></returns>
        public static IEnumerable<Error> ToErrors(this IEnumerable<SpecResult> specResults, string category = "")
        {
            Guard.ArgumentNotNull(specResults, nameof(specResults));

            foreach (var specResult in specResults)
            {
                if (!specResult.IsValid)
                {
                    yield return new Error(specResult.DetailedErrorMessage, category);
                }
            }
        }
    }
}