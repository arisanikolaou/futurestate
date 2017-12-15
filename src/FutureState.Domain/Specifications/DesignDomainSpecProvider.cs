using FutureState.Specifications;

namespace FutureState.Domain.Specifications
{
    public class DesignDomainSpecProvider : SpecProvider<DesignDomain>
    {
        public DesignDomainSpecProvider()
        {
            this.Add((domain) =>
            {
                if (domain.ParentId == domain.Id)
                    return new SpecResult($"The design domain {domain.DisplayName} cannot reference itself in its own lineage.");

                return SpecResult.Success;
            },
            "ParentId",
            "A design domain cannot reference itself in its own lineage.");
        }
    }
}
