#region

using System;
using System.Diagnostics;

#endregion

namespace FutureState.Diagnostics
{
    /// <summary>
    /// Used to capture the delta in system resource usage running any given action.
    /// </summary>
    public class ResourceMonitorLight
    {
        private readonly Stopwatch _sw;

        private double _avgPercCpuTime;

        private long _gcBytesUsedOnStart;

        private bool? _hasStopped;

        private double _memUsedOnStart;

        private double _privateMbUsed;

        private TimeSpan _processTime;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceMonitorLight" /> class.
        /// </summary>
        public ResourceMonitorLight()
        {
            _sw = new Stopwatch();
        }

        /// <summary>
        /// Gets the average percentage cpu usage between start and end. This metric is not particularly
        /// reliable on multi-threaded machines using hyper threading.
        /// </summary>
        public double AverageCpuPercUse
        {
            get
            {
                if (!_hasStopped.HasValue || !_hasStopped.Value)
                {
                    throw new InvalidOperationException("Stop has not been called.");
                }

                return _avgPercCpuTime;
            }
        }

        /// <summary>
        /// Gets the time span between start and end.
        /// </summary>
        public TimeSpan Elapsed
        {
            get
            {
                if (!_hasStopped.HasValue || !_hasStopped.Value)
                {
                    throw new InvalidOperationException("Stop has not been called.");
                }

                return _sw.Elapsed;
            }
        }

        /// <summary>
        /// Gets the total GC megabytes used.
        /// </summary>
        public double GcMbUsed { get; private set; }

        /// <summary>
        /// Gets the increased between the time Start and End was called.
        /// </summary>
        public double PrivateMbUsed
        {
            get
            {
                if (!_hasStopped.HasValue || !_hasStopped.Value)
                {
                    throw new InvalidOperationException("Stop has not been called.");
                }

                return _privateMbUsed;
            }
        }

        /// <summary>
        /// Gets the current mb used by the process.
        /// </summary>
        public double GetCurrentPrivateMb()
        {
            _privateMbUsed = Convert.ToDouble(Process.GetCurrentProcess().PrivateMemorySize64 - _memUsedOnStart)/1024.00/
                             1024.00;

            return _privateMbUsed;
        }

        /// <summary>
        /// Starts monitoring system resources.
        /// </summary>
        public void Start()
        {
            _hasStopped = false;

            // force garbage collection to analyze memory delta
            _gcBytesUsedOnStart = GC.GetTotalMemory(true); // will force garbage collection

            // always call get current process
            // don't use app domain private mb used
            var process = Process.GetCurrentProcess();

            _memUsedOnStart = process.PrivateMemorySize64;

            _processTime = process.TotalProcessorTime;

            _avgPercCpuTime = 0;

            _sw.Start();
        }

        /// <summary>
        /// Stops monitoring and calculates approximate system resource usage.
        /// </summary>
        public void Stop()
        {
            _hasStopped = true;

            // stop timer before collecting performance statistics
            _sw.Stop();

            // to collect full size of in memory db
            var _gcBytesEnd = GC.GetTotalMemory(true); // will force gc

            var process = Process.GetCurrentProcess();

            // reported in bytes
            _privateMbUsed = Convert.ToDouble(process.PrivateMemorySize64 - _memUsedOnStart)/1024.00/1024.00;

            GcMbUsed = (_gcBytesEnd - _gcBytesUsedOnStart)*1.0/1024.00/1024.00;

            // calc average cpu time - get the delta
            var processTime = process.TotalProcessorTime - _processTime;

            // devide by each processor count
            _avgPercCpuTime = processTime.Ticks/(double) _sw.Elapsed.Ticks*100.00/(Environment.ProcessorCount*1.0);
        }

        /// <summary>
        /// Displays the resource usage.
        /// </summary>
        public override string ToString()
        {
            if (_hasStopped.HasValue && _hasStopped.Value)
            {
                return string.Format(
                    "Avg Cpu Time {0:n2}%{1}MB Used {2:n2}{1}Time {3} GC {4:n2} MB ",
                    AverageCpuPercUse,
                    Environment.NewLine,
                    PrivateMbUsed,
                    Elapsed,
                    GcMbUsed);
            }

            return "Resource usage is unavailable.";
        }
    }
}