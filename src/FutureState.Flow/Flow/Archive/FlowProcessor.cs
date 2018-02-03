using System;
using System.Collections.Generic;

namespace FutureState.Flow
{
    public class FlowProcessor : FlowProcessor<object, object>
    {
        public FlowProcessor(Func<FlowProcessor<object, object>, FlowProcessState> process) : base(process)
        {
        }
    }

    public class FlowProcessor<TEntityOut, TEntityIn>
    {
        private readonly Func<FlowProcessor<TEntityOut, TEntityIn>, FlowProcessState> _processFunction;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public FlowProcessor(
            Func<FlowProcessor<TEntityOut, TEntityIn>, FlowProcessState> process)
        {
            _processFunction = process ?? throw new ArgumentNullException(nameof(process));
        }

        /// <summary>
        ///     Gets the port source(s) data driving the current processor.
        /// </summary>
        public List<FlowPortSource<TEntityIn>> PortSources { get; set; } = new List<FlowPortSource<TEntityIn>>();

        /// <summary>
        ///     Gets the processors configuration.
        /// </summary>
        public ProcessorConfiguration Configuration { get; set; }

        /// <summary>
        ///     Raised when the processor has completed.
        /// </summary>
        public event EventHandler ProcessCompleted;

        /// <summary>
        ///     Pulls data from a given query source.
        /// </summary>
        /// <returns></returns>
        public virtual FlowProcessState Process()
        {
            // load configuration

            // stream in the window of data

            // transform/process and/or validate

            // submit to destination / targets (successful packages)

            // record any response

            // get next flowPackage

            // error management

            var result = _processFunction(this);

            ProcessCompleted?.Invoke(this, new EventArgs());

            return result;
        }

        public void LoadConfiguration()
        {
            // load configuration
        }
    }
}