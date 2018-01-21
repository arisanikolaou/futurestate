using System;
using System.Collections.Generic;

namespace FutureState.Flow.Core
{
    /// <summary>
    ///     Processes data from an incoming data source.
    /// </summary>
    public interface IProcessorHandler
    {
        /// <summary>
        ///     Gets a list of warnings processing from the incoming data source.
        /// </summary>
        List<string> Warnings { get; }

        /// <summary>
        ///     Action to commit a process operation.
        /// </summary>
        Action Commit { get; }

        /// <summary>
        ///     Gets the index of the current position processing from a given data source.
        /// </summary>
        int Current { get; }

        /// <summary>
        ///     Gets the date, in utc, a process operation was started.
        /// </summary>
        DateTime StartTime { get; }
    }
}