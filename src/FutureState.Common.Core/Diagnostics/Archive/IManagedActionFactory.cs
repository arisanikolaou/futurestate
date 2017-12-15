#region

using System;
using NLog;

#endregion

namespace FutureState.Diagnostics
{
    /// <summary>
    /// An abstract factory used to create a set of <see cref="IManagedAction" />.
    /// </summary>
    public interface IManagedActionFactory
    {
        /// <summary>
        /// Gets/sets the logger to inject into sub-system classes.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Raised if the assigned value is null.
        /// </exception>
        Logger Logger { get; set; }

        /// <summary>
        /// Creates a managed object using the associated Logger instance.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        /// Raised if the assigned action is null.
        /// </exception>
        IManagedAction Create(Action action, string name = null, object tag = null);
    }
}