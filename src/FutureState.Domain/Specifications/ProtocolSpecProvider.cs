using FutureState.Specifications;

namespace FutureState.Domain.Specifications
{
    public class ProtocolSpecProvider : SpecProvider<Protocol>
    {
        public ProtocolSpecProvider()
        {
            this.Add((protocol) =>
            {
                if (protocol.ParentId == protocol.Id)
                    return new SpecResult($"Protocol {protocol.DisplayName} cannot reference itself in its own lineage. Protocol id and container id are the same.");

                return SpecResult.Success;
            },
            "ParentId",
            "A protocol cannot reference itself in its own lineage.");
        }
    }
}
