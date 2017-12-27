#region

using System;

#endregion

namespace FutureState
{
    public static class DoubleExtensions
    {
        public const double Epsilon = 1e-16;

        public static bool IsEqual(this double doubleValue, double anotherValue)
        {
            return Math.Abs(doubleValue - anotherValue) < Epsilon;
        }

        public static bool IsNotEqual(this double doubleValue, double anotherValue)
        {
            return !doubleValue.IsEqual(anotherValue);
        }

        public static bool IsNotZero(this double doubleValue)
        {
            return !doubleValue.IsZero();
        }

        public static bool IsNotZero(this double doubleValue, double epsilon)
        {
            return !doubleValue.IsZero(epsilon);
        }

        public static bool IsNullOrNaNOrZero(this double? target)
        {
            return !target.HasValue || IsZeroOrNaN(target.Value);
        }

        public static bool IsValid(this double? value)
        {
            return value.HasValue && !double.IsNaN(value.Value) && !double.IsInfinity(value.Value);
        }

        public static bool IsValid(this double value)
        {
            return !double.IsNaN(value) && !double.IsInfinity(value);
        }

        /// <summary>
        ///     This is similar to '==' operator, bu with the tolerance
        ///     This will eliminate Resharper warnings on 'double == 0'
        /// </summary>
        /// <returns></returns>
        public static bool IsZero(this double doubleValue)
        {
            return Math.Abs(doubleValue - 0.0) < Epsilon;
        }

        public static bool IsZero(this double doubleValue, double epsilon)
        {
            return Math.Abs(doubleValue - 0.0) < epsilon;
        }

        public static bool IsZeroOrNaN(this double doubleValue)
        {
            return double.IsNaN(doubleValue) || Math.Abs(doubleValue - 0.0) < Epsilon;
        }

        public static double? Sum(double? a, double? b)
        {
            if (!a.HasValue && !b.HasValue)
                return a;

            var aNum = a ?? 0; // if a has a value, assign it to aNum, if not assign 0 to aNum
            var bNum = b ?? 0; // same thing for b

            return aNum + bNum;
        }

        public static double Sum(double? a, double? b, double defualt)
        {
            if (!a.HasValue && !b.HasValue)
                return defualt;

            var aNum = a.HasValue ? a.Value : 0; // if a has a value, assign it to aNum, if not assign 0 to aNum
            var bNum = b.HasValue ? b.Value : 0; // same thing for b

            return aNum + bNum;
        }

        /// <summary>
        ///     Parser that processes % and bps
        /// </summary>
        public static double ParseEx(string input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input), "Cannot convert null to double.");

            var length = input.Length;
            input = input.Replace("%", string.Empty);
            // check length difference to avoid strings with multiple %
            if (input.Length == length - 1)
                return double.Parse(input) / 100;
            // bps
            input = input.ToLower().Replace("bps", string.Empty);
            if (input.Length == length - 3)
                return double.Parse(input) / 10000;

            return double.Parse(input);
        }
    }
}