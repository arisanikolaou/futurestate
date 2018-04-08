using System;
using System.IO;

namespace FutureState.Flow.Controllers
{
    /// <summary>
    ///     Controls how batches of data flow into flow file processors.
    /// </summary>
    /// <remarks>
    ///     Ensures that only unique files are pulled from an incoming data source.
    /// </remarks>
    public interface IFlowFileController : IDisposable
    {
        /// <summary>
        ///     Gets the type of entity being processed.
        /// </summary>
        FlowEntity SourceEntityType { get; }

        /// <summary>
        ///     Gets the output entity type to produce.
        /// </summary>
        FlowEntity TargetEntityType { get; }

        /// <summary>
        ///     Gets the display name of the flow file controller.
        /// </summary>
        string ControllerName { get; set; }

        /// <summary>
        ///     Gets the flow.
        /// </summary>
        FlowId Flow { get; set; }

        /// <summary>
        ///     Gets the configuration used by the instance.
        /// </summary>
        IProcessorConfiguration Config { get; }

        /// <summary>
        ///     Processes an incoming flow file.
        /// </summary>
        /// <param name="flowFile">The flow file to process.</param>
        /// <param name="flowBatch">The current batch process running.</param>
        /// <returns></returns>
        FlowSnapshot Process(FileInfo flowFile, FlowBatch flowBatch);

        /// <summary>
        ///     Initializes the controller.
        /// </summary>
        void Initialize();
    }
}