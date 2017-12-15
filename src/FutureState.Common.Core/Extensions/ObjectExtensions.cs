using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace FutureState
{
    public static class ObjectExtensions
    {
        public static TInput Do<TInput>(this TInput obj, Action<TInput> action) where TInput : class
        {
            if (obj == null)
            {
                return default(TInput);
            }

            action?.Invoke(obj);

            return obj;
        }

        public static int GetHashCodeSafe<T>(this T obj)
        {
            return Equals(obj, default(T)) ? 0 : obj.GetHashCode();
        }

        public static object GetInstanceFieldValue(this object poco, string name, bool isPrivate = true,
            bool ignoreCase = false)
        {
            Guard.ArgumentNotNull(poco, nameof(poco));
            Guard.ArgumentNotNull(name, nameof(name));

            return GetValue(poco.GetType(), poco, name, false, isPrivate, ignoreCase);
        }

        public static object GetStaticFieldValue(this Type type, string name, bool isPrivate = true,
            bool ignoreCase = false)
        {
            Guard.ArgumentNotNull(type, nameof(type));
            Guard.ArgumentNotNull(name, nameof(name));

            return GetValue(type, null, name, true, isPrivate, ignoreCase);
        }

        public static TInput If<TInput>(this TInput obj, Func<TInput, bool> evaluator) where TInput : class
        {
            if (obj == null)
            {
                return default(TInput);
            }

            if (!evaluator(obj))
            {
                return default(TInput);
            }

            return obj;
        }

        public static TResult Return<TInput, TResult>(this TInput obj, Func<TInput, TResult> evaluator,
            TResult failureValue) where TInput : class
        {
            if (obj == null)
            {
                return failureValue;
            }

            return evaluator(obj);
        }

        public static TResult Return<TInput, TResult>(this TInput obj, Func<TInput, TResult> evaluator)
            where TInput : class
        {
            return Return(obj, evaluator, default(TResult));
        }

        public static TInput Unless<TInput>(this TInput obj, Func<TInput, bool> evaluator) where TInput : class
        {
            if (obj == null)
            {
                return default(TInput);
            }

            if (!evaluator(obj))
            {
                return obj;
            }

            return default(TInput);
        }

        public static TResult With<TInput, TResult>(this TInput obj, Func<TInput, TResult> evaluator)
            where TInput : class
            where TResult : class
        {
            if (obj == null)
            {
                return default(TResult);
            }

            return evaluator?.Invoke(obj);
        }

        private static object GetValue(Type type, object o, string name, bool isStatic = false, bool isPrivate = true,
            bool ignoreCase = false)
        {
            var flags = BindingFlags.Default;
            flags |= isPrivate ? BindingFlags.NonPublic : BindingFlags.Public;
            flags |= isStatic ? BindingFlags.Static : BindingFlags.Instance;
            if (ignoreCase)
            {
                flags |= BindingFlags.IgnoreCase;
            }

            var memberInfo = type.GetMember(name, flags).SingleOrDefault();

            var field = memberInfo as FieldInfo;
            if (field != null)
            {
                return field.GetValue(o);
            }

            var property = memberInfo as PropertyInfo;
            if (property != null)
            {
                return property.GetValue(o);
            }

            throw new InvalidDataException("Invalid property name");
        }
    }
}