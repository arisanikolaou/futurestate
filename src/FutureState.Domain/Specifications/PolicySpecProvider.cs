using FutureState.Specifications;

namespace FutureState.Domain.Specifications
{
    public class PolicySpecProvider : SpecProvider<Policy>
    {
        public PolicySpecProvider()
        {
            this.Add((policy) =>
            {
                if (policy.DateRemoved.HasValue)
                    if (policy.DateAdded > policy.DateRemoved)
                        return new SpecResult($"Date Added {policy.DateAdded} cannot be a date in the future from Date Removed {policy.DateRemoved}.");

                return SpecResult.Success;
            },
            "Date Added",
            "Date Added cannot be a date in the future from Date Removed.");

            this.Add((policy) =>
            {
                if (policy.ContainerId == policy.Id)
                    return new SpecResult($"A policy cannot reference itself in its own lineage. Policy id and container id are the same.");

                return SpecResult.Success;
            },
            "ContainerId",
            "A policy cannot reference itself in its own lineage.");
        }
    }
}
