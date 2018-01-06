using System;

namespace FutureState.Flow
{
    /// <summary>
    ///     A well known data flow.
    /// </summary>
    public class Flow
    {
        public Guid CorrelationId { get; set; }

        public string DisplayName { get; set; }

        public DateTime DateCreated { get; set; }
    }
}