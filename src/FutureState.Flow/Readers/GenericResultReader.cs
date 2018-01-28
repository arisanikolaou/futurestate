using System;
using System.Collections.Generic;
using FutureState.Flow.Core;

namespace FutureState.Flow
{
    /// <summary>
    ///     Reads the entities successfully processed by another process.
    /// </summary>
    /// <typeparam name="TProcessResult">The process result (flow file).</typeparam>
    /// <typeparam name="TEntityIn">The entity type that was read in to produce the output.</typeparam>
    /// <typeparam name="TEntityOut">The output entity type.</typeparam>
    public class ProcessResultReader<TProcessResult, TEntityIn, TEntityOut> : IReader<TEntityIn>
        where TProcessResult : ProcessResult<TEntityIn, TEntityOut>
    {
        public IEnumerable<TEntityIn> Read(string dataSource)
        {
            var repoository = new ProcessResultRepository<TProcessResult>(dataSource);

            var processResult = repoository.Get(dataSource);

            return processResult.Input;
        }
    }

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