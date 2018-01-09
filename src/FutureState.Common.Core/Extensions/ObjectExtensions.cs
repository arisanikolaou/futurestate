using System;

namespace FutureState
{
    public static class ObjectExtensions
    {
        public static TInput Do<TInput>(this TInput obj, Action<TInput> action) where TInput : class
        {
            if (obj == null)
                return default(TInput);

            action?.Invoke(obj);

            return obj;
        }

        public static int GetHashCodeSafe<T>(this T obj)
        {
            return Equals(obj, default(T)) ? 0 : obj.GetHashCode();
        }

        public static TInput If<TInput>(this TInput obj, Func<TInput, bool> evaluator) where TInput : class
        {
            if (obj == null)
                return default(TInput);

            if (!evaluator(obj))
                return default(TInput);

            return obj;
        }

        public static TResult Return<TInput, TResult>(this TInput obj, Func<TInput, TResult> evaluator,
            TResult failureValue) where TInput : class
        {
            if (obj == null)
                return failureValue;

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
                return default(TInput);

            return !evaluator(obj) ? obj : default(TInput);
        }

        public static TResult With<TInput, TResult>(this TInput obj, Func<TInput, TResult> evaluator)
            where TInput : class
            where TResult : class
        {
            if (obj == null)
                return default(TResult);

            return evaluator?.Invoke(obj);
        }
    }
}