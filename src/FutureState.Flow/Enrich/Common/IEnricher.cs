using System;
using System.Collections.Generic;

namespace FutureState.Flow.Enrich
{
    public interface IEnricher
    {
        string UniqueId { get; set; }
    }

    public interface IEnricher<TComposite> : IEnricher
    {
        IEnumerable<IEquatable<TComposite>> Get();

        TComposite Enrich(IEquatable<TComposite> part, TComposite whole);

        IEnumerable<IEquatable<TComposite>> Find(TComposite composite);
    }
}