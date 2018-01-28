using System;
using System.IO;
using FutureState.Flow.Model;

namespace FutureState.Flow
{
    /// <summary>
    ///     Controls how batches of data flow into batch processors for processing (loading and/or transformation).
    /// </summary>
    public interface IFlowFileBatchController
    {
        /// <summary>
        ///     Gets the display name of the processor.
        /// </summary>
        string ControllerName { get; }

        /// <summary>
        ///     Gets the process id.
        /// </summary>
        Guid FlowId { get; }

        /// <summary>
        ///     Processes a batch of data from an incoming flow file within a batch process.
        /// </summary>
        /// <param name="flowFile">The flow file to process.</param>
        /// <param name="process">The batch processor.</param>
        /// <returns></returns>
        ProcessResult Process(FileInfo flowFile, BatchProcess process);

        /// <summary>
        ///     Gets the next batch to process.
        /// </summary>
        /// <param name="log">The log containing the list of files processed.</param>
        /// <returns></returns>
        FileInfo GetNextFlowFile(FlowFileLog log);
    }
}