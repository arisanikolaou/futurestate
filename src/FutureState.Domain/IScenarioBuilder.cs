using System;

namespace FutureState.Domain
{
    public interface IScenarioBuilder
    {
        Type EntityType { get; }
        void CopyToScenario(Scenario scenario, Scenario target);
        void Remove(Scenario scenario);
    }
}