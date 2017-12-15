#region

using System;

#endregion

namespace FutureState.ComponentModel
{
    /// <summary>
    /// Responsible for copying/mapping one object type into another object type.
    /// </summary>
    /// <typeparam name="TFrom">The source object type.</typeparam>
    /// <typeparam name="TTo">The target object type.</typeparam>
    public interface IMapper<in TFrom, out TTo>
    {
        TTo Map(TFrom from);
    }

    /// <summary>
    /// Responsible for copying/mapping one object type into another instance
    /// </summary>
    /// <typeparam name="TType">The source and target object type.</typeparam>
    public interface IMapper<TType>
    {
        TType Map(TType from);
    }

    //todo: revisit design and make use of generics

    public interface IMapper
    {
        /// <summary>
        /// Maps a source object of a given type to a destination object of a given type.
        /// </summary>
        object Map(object src, Type srcType, Type dstType, object dst = null);
    }
}