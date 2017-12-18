#region

using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

#endregion

namespace FutureState.Specifications
{
    public class SpecEventArgs<TEntityOrService> : EventArgs
    {
        private readonly ConcurrentBag<ISpecification<TEntityOrService>> _specs;

        public SpecEventArgs()
        {
            _specs = new ConcurrentBag<ISpecification<TEntityOrService>>();
        }

        public IProducerConsumerCollection<ISpecification<TEntityOrService>> Specs => _specs;

        public void Add(
            Expression<Func<TEntityOrService, bool>> condition,
            Func<TEntityOrService, string> detailedMessage,
            string key,
            string specDescription = "")
        {
            Guard.ArgumentNotNull(condition, nameof(condition));
            Guard.ArgumentNotNull(detailedMessage, nameof(detailedMessage));
            Guard.ArgumentNotNullOrEmpty(key, nameof(key));

            _specs.Add(
                new Specification<TEntityOrService>(
                    m =>
                    {
                        if (!condition.Compile().Invoke(m))
                            return new SpecResult(detailedMessage?.Invoke(m));

                        return SpecResult.Success;
                    },
                    key,
                    specDescription));
        }
    }
}