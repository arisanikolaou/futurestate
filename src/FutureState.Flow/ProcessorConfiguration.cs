using System;

namespace FutureState.Flow
{
    public class ProcessorConfiguration
    {
        /// <summary>
        ///     Gets the max number of entities to process from a given data source.
        /// </summary>
        public int WindowSize { get; set; }

        /// <summary>
        ///     Gets the base path to output staging files to.
        /// </summary>
        public string FlowDirPath { get; set; }

        /// <summary>
        ///     Gets the number of seconds to poll for new data from an incoming source.
        /// </summary>
        public int PollTime { get; set; }

        /// <summary>
        ///     Gets whether or not a processor should fail on the first error.
        /// </summary>
        public bool FailOnError { get; set; }

        /// <summary>
        ///     Gets/sets the id of the processor this configuration is valid for.
        /// </summary>
        public string ProcessorId { get; set; }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public ProcessorConfiguration()
        {


        }

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        /// <param name="processorId">The process id the configuration is attached to.</param>
        /// <param name="pollTime">The flow poll time in seconds.</param>
        /// <param name="windowSize"></param>
        /// <param name="baseDirectory"></param>
        public ProcessorConfiguration(string processorId, int pollTime = 1, int windowSize = 1000, string baseDirectory = null)
        {
            ProcessorId = processorId;
            PollTime = pollTime;
            WindowSize = windowSize;
            FlowDirPath = baseDirectory ?? Environment.CurrentDirectory;
        }
    }
}