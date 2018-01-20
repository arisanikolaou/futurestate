using System.Collections.Generic;

namespace FutureState.Flow.Core
{
    public interface IReader<out TEntity>
    {
        IEnumerable<TEntity> Read();
    }

    public interface IReader
    {

    }
}