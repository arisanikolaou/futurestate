#region

using System;

#endregion

namespace FutureState.Extensions
{
    public static class StringComparerExtensions
    {
        public static int GetHashCodeSafe(this StringComparer comparer, string @string)
        {
            return @string == null ? 0 : comparer.GetHashCode(@string);
        }
    }
}