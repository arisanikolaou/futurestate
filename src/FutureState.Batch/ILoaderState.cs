using System;
using System.Collections.Generic;

namespace FutureState.Batch
{
    /// <summary>
    ///     Loader process state data.
    /// </summary>
    public interface ILoaderState<out TLoadStateData> : ILoaderState
    {
        TLoadStateData Valid { get; }
    }

    /// <summary>
    ///     Loader process state data.
    /// </summary>
    public interface ILoaderState
    {
        /// <summary>
        ///     Gets/sets the number of entities added.
        /// </summary>
        long Added { get; set; }

        /// <summary>
        ///     Gets the number of entities updated.
        /// </summary>
        long Updated { get; set; }

        /// <summary>
        ///     Gets the number of entities deleted.
        /// </summary>
        long Removed { get; set; }

        /// <summary>
        ///     Gets the list of warnings.
        /// </summary>
        List<string> Warnings { get; }

        /// <summary>
        ///     Gets the current entity read.
        /// </summary>
        int CurrentRow { get; }

        /// <summary>
        ///     Gets the date and time the loader started.
        /// </summary>
        DateTime StartTime { get; }

        /// <summary>
        ///     Gets the list of errors encountered loading the data.
        /// </summary>
        IList<Exception> Errors { get; }

        /// <summary>
        ///     Gets an optional tag to attach to the portfolio.
        /// </summary>
        object Tag { get; set; }
    }
}
