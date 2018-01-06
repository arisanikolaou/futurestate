#region

using System;

#endregion

namespace FutureState
{
    /// <summary>
    ///     The default implementation of the <see cref="ICurrentDateProvider" /> interface.
    /// </summary>
    public sealed class CurrentDateProvider : ICurrentDateProvider
    {
        /// <summary>
        ///     Gets the current system's Utc date and time.
        /// </summary>
        public DateTime GetUtcNow()
        {
            return DateTime.UtcNow;
        }
    }

    /// <summary>
    ///     An abstraction on the system's static date provider.
    /// </summary>
    public interface ICurrentDateProvider
    {
        DateTime GetUtcNow();
    }
}