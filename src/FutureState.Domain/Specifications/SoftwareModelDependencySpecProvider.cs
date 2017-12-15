using FutureState.Specifications;

namespace FutureState.Domain.Specifications
{
    public class SoftwareModelDependencySpecProvider : SpecProvider<SoftwareModelDependency>
    {
        public SoftwareModelDependencySpecProvider()
        {
            this.Add((entity) =>
            {
                if (entity.SoftwareModelDependencyId == entity.SoftwareModelId)
                    return new SpecResult($"Software Models cannot depend on themselves.");

                return SpecResult.Success;
            },
            "SoftwareModelDependencyId",
            "Software Models cannot depend on themselves.");
        }
    }
}
