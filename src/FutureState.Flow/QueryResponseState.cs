using System;

namespace FutureState.Flow
{
    public class QueryResponseState
    {
        public string ConsumerId { get; set; }

        public int LocalIndex { get; set; }

        public Guid CheckPoint { get; set; }

        public Guid FlowId { get; set; }
    }
}