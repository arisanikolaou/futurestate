using System;
using System.Collections.Generic;
using NLog;

namespace FutureState.Flow
{
    public class Processor : Processor<object,object>
    {
        public Processor(Func<Processor<object, object>, ProcessState> process) : base(process)
        {
        }
    }

    public class Processor<TEntityOut,TEntityIn>
    {
        readonly Func<Processor<TEntityOut, TEntityIn>, ProcessState> _processFunction;

        /// <summary>
        ///     Gets the port source(s) data driving the current processor.
        /// </summary>
        public List<PortSource<TEntityIn>> PortSources { get; set; } = new List<PortSource<TEntityIn>>();

        /// <summary>
        ///     Gets the processors configuration.
        /// </summary>
        public ProcessorConfiguration Configuration { get; set; }

        /// <summary>
        ///     Raised when the processor has completed.
        /// </summary>
        public event EventHandler ProcessCompleted;

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public Processor(Func<Processor<TEntityOut, TEntityIn>, ProcessState> process)
        {
            _processFunction = process ?? throw new ArgumentNullException(nameof(process));
        }

        public virtual ProcessState Process()
        {
            // load configuration

            // stream in the window of data

            // transform/process and/or validate

            // submit to destination / targets (successful packages)

            // record any response

            // get next package

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
