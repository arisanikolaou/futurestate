using System;
using System.Collections.Generic;

namespace FutureState.Flow.Enrich
{
    public interface IEnricher
    {
        string UniqueId { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="TTarget">The data type to enrich.</typeparam>
    public interface IEnricher<TTarget> : IEnricher
    {
        IEnumerable<IEquatable<TTarget>> Get();

        TTarget Enrich(IEquatable<TTarget> part, TTarget whole);

        IEnumerable<IEquatable<TTarget>> Find(TTarget composite);
    }
}