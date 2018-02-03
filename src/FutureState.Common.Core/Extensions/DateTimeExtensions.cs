#region

using System;

#endregion

namespace FutureState
{
    public static class DateTimeExtensions
    {
        public enum ForceStrategy
        {
            Unknown,

            ForceAll,

            ConvertLocalAndForceUnspecified
        }

        public static DateTime AsUnspecifiedKind(this DateTime value)
        {
            var unspecifiedDate = DateTime.SpecifyKind(value.Date, DateTimeKind.Unspecified);

            return unspecifiedDate;
        }

        public static DateTime? AsUnspecifiedKind(this DateTime? value)
        {
            var unspecifiedDate = value.HasValue
                ? DateTime.SpecifyKind(value.Value.Date, DateTimeKind.Unspecified)
                : (DateTime?) null;

            return unspecifiedDate;
        }

        public static DateTime ForceUtc(this DateTime value)
        {
            if (value.Kind == DateTimeKind.Utc)
                return value;

            return new DateTime(value.Ticks, DateTimeKind.Utc);
        }

        public static DateTime ForceUtc(this DateTime value, ForceStrategy forceStrategy)
        {
            switch (forceStrategy)
            {
                case ForceStrategy.ForceAll:
                    return ForceUtc(value);

                case ForceStrategy.ConvertLocalAndForceUnspecified:
                    return value.Kind == DateTimeKind.Local
                        ? value.ToUniversalTime()
                        : value.ForceUtc();

                default:
                    throw new NotSupportedException($"Unsupported strategy: {forceStrategy}.");
            }
        }

        /// <summary>
        ///     Similar to Magnum's ForceUtc but works with nullable DateTime
        /// </summary>
        public static DateTime? ForceUtc(this DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.ForceUtc() : default(DateTime?);
        }

        /// <summary>
        ///     Similar to Magnum's ForceUtc but works with nullable DateTime
        ///     and strips out time
        /// </summary>
        public static DateTime? ForceUtcDate(this DateTime? dateTime)
        {
            return dateTime.HasValue ? dateTime.Value.Date.ForceUtc() : default(DateTime?);
        }

        /// <summary>
        ///     Similar to Magnum's ForceUtc
        ///     and strips out time
        /// </summary>
        public static DateTime ForceUtcDate(this DateTime dateTime)
        {
            return dateTime.Date.ForceUtc();
        }

        /// <summary>
        ///     Gets the number of months between two dates.
        ///     e.g. this Apr 2012, and Another is Mar 2011, should return 11 months
        /// </summary>
        /// <returns></returns>
        public static int MonthDiff(this DateTime start, DateTime end)
        {
            return end.Month - start.Month + 12 * (end.Year - start.Year);
        }
    }
}