using System;
using System.Collections.Generic;

namespace FutureState.Batch
{
    /// <summary>
    ///     Loader process state data.
    /// </summary>
    /// <typeparam name="TLoadStateData">The type of payload to carry in load state.</typeparam>
    public class LoaderState<TLoadStateData> : ILoaderState<TLoadStateData>
        where TLoadStateData : new()
    {
        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public LoaderState()
        {
            StartTime = DateTime.UtcNow;
            Valid = new TLoadStateData();
        }

        /// <summary>
        ///     Gets the loaders end time.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        ///     Gets an optional tag to use.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        ///     Gets/sets the load state data.
        /// </summary>
        public TLoadStateData Valid { get; set; }

        /// <summary>
        ///     Gets the total entities added.
        /// </summary>
        public long Added { get; set; }

        /// <summary>
        ///     Gets the total entities updated.
        /// </summary>
        public long Updated { get; set; }

        /// <summary>
        ///     Gets the total entities removed.
        /// </summary>
        public long Removed { get; set; }

        /// <summary>
        ///     Gets the total warnings.
        /// </summary>
        public List<string> Warnings { get; }

        /// <summary>
        ///     Gets the active row being read.
        /// </summary>
        public int CurrentRow { get; internal set; }

        /// <summary>
        ///     Gets the loader's start time.
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        ///     Gets the number of errors encountered processing data from a given data source.
        /// </summary>
        public long ErrorsCount { get; set; }

        /// <summary>
        ///     Gets the number of warnings processing data from a given data source.
        /// </summary>
        public long WarningsCount { get; set; }

        /// <summary>
        ///     Gets the number of batches processed.
        /// </summary>
        public long Batches { get; set; }

        /// <summary>
        ///     Reports the state's added/updated and removed entities.
        /// </summary>
        public override string ToString()
        {
            TimeSpan ts = TimeSpan.Zero;
            if (EndTime.HasValue)
                ts = EndTime.Value - StartTime;

            // added/updated
            return
                $"Added {Added} entities. Updated {Updated}. Removed {Removed}.  Total errors {this.ErrorsCount}. Total time; {ts.TotalSeconds:n2} seconds.";
        }
    }
}
