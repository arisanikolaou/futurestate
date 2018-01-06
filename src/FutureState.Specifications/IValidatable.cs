#region

using System.Collections.Generic;

#endregion

namespace FutureState.Specifications
{
    /// <summary>
    ///     An entity or service that can validate its own state.
    /// </summary>
    public interface IValidatable
    {
        /// <summary>
        ///     Gets a list of errors detected in the current instance.
        /// </summary>
        IEnumerable<Error> Validate();
    }
}