using System;
using System.Collections.Generic;

namespace FutureState.Flow
{
    /// <summary>
    ///     Gets the result of processing data from an incoming data source.
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        ///     Gets the time time spent processing from the incoming data source.
        /// </summary>
        public TimeSpan ProcessTime { get; set; }

        /// <summary>
        ///     Gets any errors encountered processing from the incoming data source.
        /// </summary>
        public List<Exception> Exceptions { get; set; }

        /// <summary>
        ///     Gets the warnings.
        /// </summary>
        public List<string> Warnings { get; set; }

        /// <summary>
        ///     Gets the total number of entities processed from the source.
        /// </summary>
        public long ProcessedCount { get; set; }

        /// <summary>
        ///     Gets or sets the process name.
        /// </summary>
        public string ProcessName { get; set; }

        /// <summary>
        ///     Gets the active BatchProcess used in the processed.
        /// </summary>
        public BatchProcess BatchProcess { get; set; }
    }

    /// <summary>
    ///     Gets the result from processing data from a particular type of incoming data source.
    /// </summary>
    public class ProcessResult<TEntityIn> : ProcessResult
    {
        /// <summary>
        ///     Gets the items that were used as the source for procesing such as the items in a csv file.
        /// </summary>
        public List<TEntityIn> Input { get; internal set; }

        /// <summary>
        ///     Gets the errors that were encountered processing the incoming entities.
        /// </summary>
        public List<ProcessError<TEntityIn>> Errors { get; set; }
    }

    /// <summary>
    ///     The result state from processing data from a particular incoming type to an outgoing type.
    /// </summary>
    /// <typeparam name="TEntityIn">The data source type.</typeparam>
    /// <typeparam name="TEntityOut">The output type.</typeparam>
    public class ProcessResult<TEntityIn, TEntityOut> : ProcessResult<TEntityIn>
    {
        /// <summary>
        ///     Gets the valid items created after processing. This is the primary ouput.
        /// </summary>
        public List<TEntityOut> Output { get; set; } = new List<TEntityOut>();
    }
}