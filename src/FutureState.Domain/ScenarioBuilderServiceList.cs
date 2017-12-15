using System.Collections.Generic;
using System.Linq;

namespace FutureState.Domain
{
    public class ScenarioBuilderServiceList
    {
        public List<IScenarioBuilder> List { get; }

        public ScenarioBuilderServiceList(IEnumerable<IScenarioBuilder> builders)
        {
            Guard.ArgumentNotNull(builders, nameof(builders));

            List = builders.ToList();
        }
    }
}