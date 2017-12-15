#region

using System;
using FutureState.Data;

#endregion

namespace FutureState.Diagnostics
{
    /// <summary>
    /// Represents an action that is profiled for cpu and use.
    /// </summary>
    /// <remarks>
    /// Consider calling GC.Collect before calling invoke.
    /// </remarks>
    public class ProfiledAction : ManagedActionBase
    {
        private readonly IRepository<PerformanceStat, string> _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfiledAction" /> class.
        /// </summary>
        public ProfiledAction(Action action, string name, IRepository<PerformanceStat, string> db, object tag = null)
        {
            Guard.ArgumentNotNull(db, nameof(db));

            Setup(action, name, tag);

            _db = db;
        }

        /// <summary>
        /// Invokes a unit of work and records the resources used to execute it.
        /// </summary>
        protected override void EndInvoke()
        {
            // disposed status and action assignment are
            // checked by the base class

            // new stat entry?
            var isNew = false;

            // store variable inline
            var db = _db;

            _logger.Trace("Executing {0}.".Params(Name));

            PerformanceStat stat = null;

            if (db.Any(m => m.Name == Name))
            {
                stat = db.Get(Name);
            }

            if (stat == null)
            {
                stat = new PerformanceStat {Name = Name, Tag = _tag};
                isNew = true;
            }

            var rm = new ResourceMonitorLight();
            rm.Start();

            try
            {
                _action.Invoke();
            }
            catch (Exception ex)
            {
                var msg = "Failed to execute {0}: {1}".Params(Name, ex);
                _logger.Error(msg);

                // always re-throw action
                throw;
            }
            finally
            {
                rm.Stop();

                // log the completion as well as the performance statics
                _logger.Trace("Executed {0}".Params(Name));
            }

            // assign average cpu use
            if (stat.TotalRuns == 0)
            {
                // initialize
                stat.AvgCpuUsed = rm.AverageCpuPercUse;
                stat.PrivateMbUsed = rm.PrivateMbUsed;
                stat.GcMB = rm.GcMbUsed;
            }
            else
            {
                // average out cpu use and private mb used
                stat.AvgCpuUsed = (stat.AvgCpuUsed + rm.AverageCpuPercUse)/2.00;
                stat.PrivateMbUsed = (stat.PrivateMbUsed + rm.PrivateMbUsed)/2.00;
                stat.GcMB = (stat.PrivateMbUsed + rm.GcMbUsed)/2.00;
            }

            stat.TotalRuns++;

            if (!stat.FirstRunTime.HasValue)
            {
                stat.FirstRunTime = rm.Elapsed;
            }

            stat.TotalTime = stat.TotalTime.Add(rm.Elapsed);

            if (isNew)
            {
                db.Insert(stat);
            }
            else
            {
                db.Update(stat);
            }

            _logger.Trace("Saved {0} Stats {1}", Name, stat);
        }
    }
}