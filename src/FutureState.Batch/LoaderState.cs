using System;
using System.Collections.Generic;
using System.Text;

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
            Warnings = new List<string>();
            Errors = new List<Exception>();
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
        ///     Gets the list of errors encountered loading the data.
        /// </summary>
        public IList<Exception> Errors { get; }

        /// <summary>
        ///     Reports the state's added/updated and removed entities.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            TimeSpan ts = TimeSpan.Zero;
            if (EndTime.HasValue)
                ts = EndTime.Value - StartTime;

            // added/updated
            string header =
                $"Added {Added} entities. Updated {Updated}. Removed {Removed}.  Total errors {Errors.Count}. Total time; {ts.TotalSeconds:n2} seconds.";

            var sb = new StringBuilder(header);
            if (Errors.Count > 0)
            {
                sb.AppendLine("Errors:");
                // display 1st 100 errors
                for (var i = 0; i < Errors.Count && i < 100; i++)
                    sb.AppendLine(Errors[i].Message);
            }

            if (Warnings.Count > 0)
            {
                sb.AppendLine("Warnings:");
                // display 1st 100 warnings
                for (var i = 0; i < Warnings.Count && i < 100; i++)
                    sb.AppendLine(Warnings[i]);
            }

            return sb.ToString();
        }
    }
}
