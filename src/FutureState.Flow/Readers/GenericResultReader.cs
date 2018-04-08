using System;
using System.Collections.Generic;

namespace FutureState.Flow
{
    public class GenericResultReader<TEntityOut> : IReader<TEntityOut>
    {
        private readonly Func<string, IEnumerable<TEntityOut>> _get;

        public GenericResultReader(Func<string, IEnumerable<TEntityOut>> get)
        {
            _get = get;
        }

        public IEnumerable<TEntityOut> Read(string dataSource)
        {
            return _get(dataSource);
        }
    }
}