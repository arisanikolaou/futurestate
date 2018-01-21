using System;
using System.Collections.Generic;
using System.Linq;
using EmitMapper;
using FutureState.Specifications;

namespace FutureState.Flow.Core
{
    public class ProcessorConfiguration<TEntityIn, TEntityOut> 
        where TEntityOut : class, new()
    {
        public ProcessorConfiguration(
            IProvideSpecifications<TEntityOut> specProviderForEntity = null,
            IProvideSpecifications<IEnumerable<TEntityOut>> specProviderForEntityCollection = null,
            IProcessResultRepository<ProcessResult> repository = null,
            ObjectsMapper<TEntityIn, TEntityOut> mapper = null)
        {
            Mapper = mapper ?? ObjectMapperManager.DefaultInstance.GetMapper<TEntityIn, TEntityOut>();

            Rules = specProviderForEntity?.GetSpecifications().ToArray() ??
                     Enumerable.Empty<ISpecification<TEntityOut>>();

            CollectionRules = specProviderForEntityCollection?.GetSpecifications().ToArray() ??
                               Enumerable.Empty<ISpecification<IEnumerable<TEntityOut>>>();

            Repository = repository ?? new ProcessResultRepository<ProcessResult>(Environment.CurrentDirectory);
        }

        public IProcessResultRepository<ProcessResult> Repository { get;  }

        public IEnumerable<ISpecification<TEntityOut>> Rules { get; }

        public ObjectsMapper<TEntityIn, TEntityOut> Mapper { get; }

        public IEnumerable<ISpecification<IEnumerable<TEntityOut>>> CollectionRules { get; }
    }
}