#region

using System.Collections.Generic;

#endregion

namespace FutureState.Specifications
{
    /// <summary>
    /// Validates a given objects against an aggregated set of rules.
    /// </summary>
    public interface IValidatorAggregate
    {
        /// <summary>
        /// The list of validation errors that was encountered testing the subject.
        /// </summary>
        /// <param name="subject">The subject to test.</param>
        /// <returns></returns>
        IEnumerable<Error> GetErrors(object subject);
    }
}