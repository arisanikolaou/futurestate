using System;
using System.Collections.Generic;

namespace FutureState.Flow
{
    public class Package : Package<object>
    {

    }

    public class Package<TEntity>
    {
        public Guid CorrelationId { get; set; }

        public int Step { get; set; }

        public string Name { get; set; }

        public List<TEntity> Data { get; set; }
    }
}
