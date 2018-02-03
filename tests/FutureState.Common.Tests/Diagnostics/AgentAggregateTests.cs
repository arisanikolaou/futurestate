using FutureState.Diagnostics;
using System.Collections.Generic;
using Xunit;

namespace FutureState.Common.Tests.Diagnostics
{
    public class AgentAggregateTests
    {
        public class TestAgent : IAgent
        {
            public bool HasStopped { get; private set; }
            public bool HasStarted { get; private set; }

            public void Start()
            {
                HasStarted = true;
                HasStopped = false;
            }

            public void Stop()
            {
                HasStarted = false;
                HasStopped = true;
            }
        }

        public class TestAgent2 : IAgent
        {
            public bool HasStopped { get; private set; }
            public bool HasStarted { get; private set; }

            public void Start()
            {
                HasStarted = true;
                HasStopped = false;
            }

            public void Stop()
            {
                HasStarted = false;
                HasStopped = true;
            }
        }

        [Fact]
        public void AgentAggregateCanStartAndStop()
        {
            var agents = new List<IAgent> { new TestAgent(), new TestAgent2() };

            // should not run two agents of the same type
            var agentAggregate = new AgentAggregate(agents);

            agentAggregate.Start();

            foreach (var agent in agents)
                Assert.True(agent.HasStarted);

            agentAggregate.Stop();

            foreach (var agent in agents)
                Assert.False(agent.HasStarted);
        }
    }
}