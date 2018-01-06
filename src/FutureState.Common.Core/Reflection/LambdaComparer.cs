#region

using System;
using System.Collections.Generic;

#endregion

namespace FutureState
{
    public class LambdaComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _lambdaComparer;

        private readonly Func<T, int> _lambdaHash;

        public LambdaComparer(Func<T, T, bool> lambdaComparer)
            : this(lambdaComparer, o => 0 /* this should use default hash code function */)
        {
        }

        public LambdaComparer(Func<T, T, bool> lambdaComparer, Func<T, int> lambdaHash)
        {
            _lambdaComparer = lambdaComparer ?? throw new ArgumentNullException(nameof(lambdaComparer));
            _lambdaHash = lambdaHash ?? throw new ArgumentNullException(nameof(lambdaHash));
        }

        public bool Equals(T x, T y)
        {
            return _lambdaComparer(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _lambdaHash(obj);
        }
    }
}