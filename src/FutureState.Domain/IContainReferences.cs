using System.Collections.Generic;

namespace FutureState.Domain
{
    public interface IContainReferences
    {
        IEnumerable<Reference> GetReferences();
    }
}