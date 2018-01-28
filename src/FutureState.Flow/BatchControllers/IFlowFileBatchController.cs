﻿using System;
using System.IO;
using FutureState.Flow.Model;

namespace FutureState.Flow.BatchControllers
{
    /// <summary>
    ///     Controls how batches of data flow into batch processors for processing (loading and/or transformation).
    /// </summary>
    public interface IFlowFileBatchController
    {
        /// <summary>
        ///     Gets the display name of the processor.
        /// </summary>
        string ControllerName { get; set; }

        /// <summary>
        ///     Gets the process id.
        /// </summary>
        Guid FlowId { get; set; }

        /// <summary>
        ///     Gets the input directory or port.
        /// </summary>
        string InDirectory { get; set; }

        /// <summary>
        ///     Gets the output directory or port.
        /// </summary>
        string OutDirectory { get; set; }

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