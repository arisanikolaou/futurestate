#region

using System;

#endregion

namespace FutureState
{
    /// <summary>
    ///     Marks a field or property to skip deep copying and use shallow copying instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SkipDeepCloneAttribute : Attribute
    {
    }
}