#region

using EmitMapper;
using EmitMapper.MappingConfiguration;
using FutureState.ComponentModel;
using System;
using System.Linq;

#endregion

namespace FutureState.Reflection
{
    /// <summary>
    ///     Common utility methods for copying data between structs and classes.
    /// </summary>
    /// <remarks>
    ///     Adds some responsibilities ontop of a normal object mapping operation.
    /// </remarks>
    public static class DefaultMapper
    {
        // replace emit mapper with AutoMapper in order to be consistent with web
        private static IMapper _default = new EmitMapperImpl();

        // the idea of having state in extension methods is problematic !
        public static IMapper Default
        {
            get => _default;
            set // since it is static property - overriding it can be problematic !! Consider redesign.
            {
                Guard.ArgumentNotNull(value, nameof(Default));

                _default = value;
            }
        }

        public static T MapFrom<T>(this T source, params object[] sources)
            where T : class
        {
            return MapFromInner(source, sources, Default);
        }

        public static T MapFrom<T>(this T source, object[] sources, IMapper mapper)
            where T : class
        {
            return MapFromInner(source, sources, mapper);
        }

        public static TTo MapStructTo<TTo>(this object source, TTo? dst = null) where TTo : struct
        {
            return MapStructToInner(source, dst, Default);
        }

        public static TTo MapStructTo<TTo>(this object source, TTo? dst, IMapper mapper) where TTo : struct
        {
            return MapStructToInner(source, dst, mapper);
        }

        public static TTo MapStructTo<TTo>(this object source, IMapper mapper) where TTo : struct
        {
            return MapStructToInner<TTo>(source, null, mapper);
        }

        public static TTo MapTo<TTo>(this object source, TTo dst = null) where TTo : class
        {
            return MapToInner(source, dst, Default);
        }

        public static TTo MapTo<TTo>(this object source, TTo dst, IMapper mapper) where TTo : class
        {
            return MapToInner(source, dst, mapper);
        }

        public static TTo MapTo<TTo>(this object source, IMapper mapper) where TTo : class
        {
            return MapToInner<TTo>(source, null, mapper);
        }

        private static T MapFromInner<T>(this T source, object[] sources, IMapper mapper)
            where T : class
        {
            if (source != null && sources.Any())
            {
                var dest = (T)mapper.Map(source, typeof(T), typeof(T));

                foreach (var src in sources.Where(s => s != null))
                {
                    var sourceType = src.GetType();
                    dest = (T)mapper.Map(src, sourceType, typeof(T), dest);
                }

                return dest;
            }

            // bug !! if source != null && sources is empty =>
            // returns source instead of clone of source
            return source;
        }

        private static TTo MapStructToInner<TTo>(object source, TTo? dst, IMapper mapper) where TTo : struct
        {
            if (source == null)
                return default(TTo);

            // type override for a destination type
            var actualType = typeof(TTo);
            if (dst.HasValue)
                actualType = dst.Value.GetType();

            return (TTo)mapper.Map(source, source.GetType(), actualType, dst.HasValue ? (object)dst.Value : null);
        }

        private static TTo MapToInner<TTo>(object source, TTo dst, IMapper mapper) where TTo : class
        {
            if (source == null)
                return default(TTo);

            // type override for a destination type
            var actualType = typeof(TTo);
            if (dst != null)
                actualType = dst.GetType();

            return (TTo)mapper.Map(source, source.GetType(), actualType, dst);
        }

        public class EmitMapperImpl : IMapper
        {
            private static readonly ObjectMapperManager _innerMapper = ObjectMapperManager.DefaultInstance;

            private static readonly DefaultMapConfig _defaultMapConfig = new DefaultMapConfig();

            public object Map(object src, Type srcType, Type dstType, object dst = null)
            {
                var mapper = _innerMapper.GetMapperImpl(srcType, dstType, _defaultMapConfig);

                return mapper.Map(src, dst, src);
            }
        }
    }
}