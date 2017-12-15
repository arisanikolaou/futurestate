#region

using FutureState.Data;

#endregion

namespace FutureState.Diagnostics
{
    /// <summary>
    /// A repository of rolled up performance statistics for a given action class.
    /// </summary>
    public class InMemoryPerformanceStatsRepository : InMemoryRepository<PerformanceStat, string>
    {
        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public InMemoryPerformanceStatsRepository()
            : base(s => s.Id)
        {
            // reserved for future use
        }

        public PerformanceStat this[string name]
        {
            get { return FirstOrDefault(m => m.Name == name); }
        }
    }
}