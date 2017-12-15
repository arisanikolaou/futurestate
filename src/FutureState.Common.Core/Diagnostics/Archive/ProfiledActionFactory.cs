#region

using System;
using NLog;

#endregion

namespace FutureState.Diagnostics
{
    /// <summary>
    /// </summary>
    /// <remarks>
    /// This factory should only be used to diagnose performance issues in the system as the overhead
    /// in executing managed actions is considerable. It also uses <see cref="InMemoryPerformanceStatsRepository" />
    /// </remarks>
    public class ProfiledActionFactory
    {
        protected static Logger _logger = LogManager.GetCurrentClassLogger();

        private InMemoryPerformanceStatsRepository _db;

        /// <summary>
        /// Creates a new instance of the profiled action factory.
        /// </summary>
        public ProfiledActionFactory()
        {
            // reserved for future use
            _db = new InMemoryPerformanceStatsRepository();
        }

        /// <summary>
        /// Gets/sets the repository to store performance statistics.
        /// </summary>
        public InMemoryPerformanceStatsRepository PerformanceDb
        {
            get { return _db; }

            set
            {
                Guard.ArgumentNotNull(value, nameof(PerformanceDb));

                _db = value;
            }
        }

        /// <summary>
        /// Creates a boxed instance of a <see cref="ProfiledAction" /> object.
        /// </summary>
        /// <param name="action">Required. The action to execute.</param>
        /// <param name="name">The name of the action.</param>
        /// <param name="tag">The action tag.</param>
        /// <returns>A boxed ProfiledAction object.</returns>
        public virtual IManagedAction Create(Action action, string name = @"Action", object tag = null)
        {
            var item = new ProfiledAction(action, name, _db, tag);

            return item;
        }

        public virtual PerformanceStat Exec(Action action, string name = @"Action", object tag = null)
        {
            var item = new ProfiledAction(action, name, _db, tag);

            item.Invoke();

            return _db.FirstOrDefault(m => m.Name == name);
        }

        public virtual PerformanceStat ExecAndLog(Action action, string name = @"Action", object tag = null)
        {
            var item = new ProfiledAction(action, name, _db, tag);

            item.Invoke();

            var result = _db.FirstOrDefault(m => m.Name == name);

            _logger.Info(result);

            return result;
        }
    }
}