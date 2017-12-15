#region

using System;
using FutureState.Data;

#endregion

namespace FutureState.Diagnostics
{
    /// <summary>
    /// A data structure used to record the performance stats of a critical system process.
    /// </summary>
    public class PerformanceStat : IEntity<string>
    {
        /// <summary>
        /// Gets the average cpu usage as a value and not as a percentage.
        /// </summary>
        public double AvgCpuUsed { get; internal set; }

        /// <summary>
        /// Gets how long it took to execute the first action.
        /// </summary>
        public TimeSpan? FirstRunTime { get; protected internal set; }

        /// <summary>
        /// Gets the total megabytes in managed memory.
        /// </summary>
        public double GcMB { get; internal set; }

        /// <summary>
        /// Gets the name of the managed action.
        /// </summary>
        public string Name { get; internal set; }

        /// <summary>
        /// Gets the private megabytes used.
        /// </summary>
        public double PrivateMbUsed { get; internal set; }

        /// <summary>
        /// Gets/sets a tag to associated with the performance counter.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets the number of times the action was invoked during the
        /// </summary>
        public int TotalRuns { get; internal set; }

        /// <summary>
        /// Gets the total time spent invoking the action by all threads.
        /// </summary>
        public TimeSpan TotalTime { get; protected internal set; }

        /// <summary>
        /// Gets the name of the performance statistic.
        /// </summary>
        public string Id => Name;

        /// <summary>
        /// Gets the average time to execute a given trial or method call.
        /// </summary>
        public TimeSpan GetAvgTimeSpan()
        {
            double totalRuns = TotalRuns == 0 ? 1 : TotalRuns;

            return TimeSpan.FromMilliseconds(TotalTime.TotalMilliseconds/totalRuns);
        }

        /// <summary>
        /// Displays the name and the average time trial.
        /// </summary>
        public override string ToString()
        {
            return "Test '{0}' {1:n2}(s) Avg Time {2:n1} Mb {3:n1}%  Cpu {4:n2}x trials GC {5:n0} MB"
                .Params(Name, GetAvgTimeSpan().TotalSeconds, PrivateMbUsed, AvgCpuUsed, TotalRuns, GcMB);
        }
    }
}