#region

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;

#endregion

namespace FutureState
{
    //an - part of the work has been ported from yunsheng's work from the spciq financial market library

    public static class StringExtension
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerNonUserCode]
        public static bool Exists(this string text)
        {
            return !string.IsNullOrWhiteSpace(text);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerNonUserCode]
        public static string GetEnumValueOrDefault<T>(this string input, T defaultValue, params Tuple<string, T>[] maps)
            where T : struct, IConvertible
        {
            string result;

            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            if (input != null)
            {
                input = input.Trim();
            }

            if (string.IsNullOrEmpty(input))
            {
                result = defaultValue.ToString(CultureInfo.InvariantCulture);
            }
            else
            {
                T output;
                if (Enum.TryParse(input, true, out output))
                {
                    result = output.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    var map = maps.FirstOrDefault(x => x.Item1.Equals(input, StringComparison.OrdinalIgnoreCase));
                    result = map != null
                        ? map.Item2.ToString(CultureInfo.InvariantCulture)
                        : defaultValue.ToString(CultureInfo.InvariantCulture);
                }
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerNonUserCode]
        public static string GetEnumValueOrDefault<T>(this string input, T defaultValue)
            where T : struct, IConvertible
        {
            if (!typeof(T).IsEnum)
            {
                throw new ArgumentException("T must be an enumerated type");
            }

            T output;
            var result = Enum.TryParse(input, true, out output)
                ? output.ToString(CultureInfo.InvariantCulture)
                : defaultValue.ToString(CultureInfo.InvariantCulture);

            return result;
        }

        /// <summary>
        /// Get string value from input. Return default value if input is null.
        /// </summary>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerNonUserCode]
        public static string GetValueOrDefault(this string input, string nullOrEmptyDefault,
            params Tuple<string, string>[] maps)
        {
            string result;

            if (input != null)
            {
                input = input.Trim();
            }

            if (string.IsNullOrEmpty(input))
            {
                result = nullOrEmptyDefault;
            }
            else
            {
                result =
                    maps.Where(x => x.Item1.Equals(input, StringComparison.OrdinalIgnoreCase))
                        .Select(x => x.Item2)
                        .FirstOrDefault() ?? input;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerNonUserCode]
        public static string Params(this string text, params object[] args)
        {
            return string.Format(text, args);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerNonUserCode]
        public static string Params(this string text, string arg)
        {
            return string.Format(text, arg);
        }
    }
}