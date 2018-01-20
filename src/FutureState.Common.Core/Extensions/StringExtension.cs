#region

using System.Diagnostics;
using System.Runtime.CompilerServices;

#endregion

namespace FutureState
{
    public static class StringExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerNonUserCode]
        public static bool Exists(this string text)
        {
            return !string.IsNullOrWhiteSpace(text);
        }
    }
}