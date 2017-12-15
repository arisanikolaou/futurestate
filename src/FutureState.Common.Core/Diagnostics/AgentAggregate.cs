namespace FutureState.Diagnostics
{
    #region usings

    using NLog;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    #endregion usings

    /// <summary>
    /// An agent which is a collection of one or more other agents.
    /// </summary>
    /// <remarks>
    /// This class isn't thread safe per se.
    /// </remarks>
    public sealed class AgentAggregate : IAgent
    {
        private static readonly Logger _logger = LogManager.GetCurrentClassLogger();

        private readonly IList<IAgent> _agents;

        private readonly IList<Exception> _errors;

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="agents">A list of zero or more distinct agents by agent type.</param>
        public AgentAggregate(IEnumerable<IAgent> agents)
        {
            Guard.ArgumentNotNull(agents, nameof(agents));

            //don't accept duplicate types
            var agentsCol = agents.ToCollection();

            _agents = agentsCol.DistinctBy(m => m.GetType().FullName).ToList();
            _errors = new List<Exception>();

            if (!_agents.Any())
            {
                _logger.Warn("No agents were added to the agent aggregate.");
            }

            if (_agents.Count() != agentsCol.Count())
            {
                _logger.Warn(
                    "Error. Non-distinct agents by type passed into agent aggregate. Only one instance per type will be started/stopped.");
            }
        }

        /// <summary>
        /// Gets the list of errors that occurred either starting or stopping the agents.
        /// </summary>
        public IEnumerable<Exception> Errors => _errors;

        public bool HasStarted { get; private set; }

        /// <summary>
        /// Starts all agents composed in the instance. Will not bubble up exception thrown
        /// while starting up these agents. Any errors will be aggregated into the errors collection.
        /// </summary>
        public void Start()
        {
            if (HasStarted)
            {
                throw new InvalidOperationException("Already started.");
            }

            _logger.Info($"Starting agents: {_agents.Count()}");

            _errors.Clear();

            // note start all agents serially .. don't use parallelism
            foreach (var agent in _agents)
            {
                try
                {
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.Debug("Starting agent: {0}", agent.GetType().FullName);
                    }

                    agent.Start();
                }
                catch (Exception ex)
                {
                    _errors.Add(ex);

                    _logger.Error(ex, $"Error: Failed to start agent: {agent.GetType().FullName}");
                }
            }

            HasStarted = true;

            _logger.Info("Started");
        }

        /// <summary>
        /// Stops all agents composed in the instance.
        /// </summary>
        public void Stop()
        {
            if (!HasStarted)
            {
                throw new InvalidOperationException("Hasn't started.");
            }

            // should assume that info level messages may be reported through the management console
            // and more detail may be appreciated.
            _logger.Info("Stopping all agents...");

            _errors.Clear();

            foreach (var agent in _agents)
            {
                try
                {
                    // this may produce a confusing log when reading from file or database
                    if (_logger.IsDebugEnabled)
                    {
                        _logger.Debug($"Stopping agent: {agent.GetType().FullName}");
                    }

                    agent.Stop();
                }
                catch (Exception x)
                {
                    _errors.Add(x);

                    _logger.Error(x, $"Failed to stop {agent.GetType()}");
                }
            }

            HasStarted = false;

            _logger.Info("Stopped.");
        }
    }
}