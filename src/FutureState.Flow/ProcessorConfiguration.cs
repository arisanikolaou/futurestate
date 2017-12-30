using System;

namespace FutureState.Flow
{
    public class ProcessorConfiguration
    {
        /// <summary>
        ///     Gets the max number of entities to process from a given data source.
        /// </summary>
        public int WindowSize { get; set; } = 100;

        /// <summary>
        ///     Gets the base path to output staging files to.
        /// </summary>
        public string FlowDirPath { get; set; } = Environment.CurrentDirectory;

        /// <summary>
        ///     Gets the number of seconds to poll for new data from an incoming source.
        /// </summary>
        public int PollTime { get; set; } = 1;

        /// <summary>
        ///     Gets whether or not a processor should fail on the first error.
        /// </summary>
        public bool FailOnError { get; set; } = false;

        /// <summary>
        ///     Gets/sets the id of the processor this configuration is valid for.
        /// </summary>
        public string Id { get; set; } = "ProcessorConfiguration";
    }
}