using System;
using System.Collections.Generic;

namespace FutureState.Flow
{
    public class ProcessState
    {
        public Guid CorrelationId { get; set; }

        public string PackageId { get; set; }

        public bool IsSuccessful { get; set; }

        public List<ErrorEvent> Errors { get; set; }
    }
}