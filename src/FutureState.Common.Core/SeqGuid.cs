using System;

namespace FutureState
{
    /// <summary>
    ///     CombGuid generator.
    /// </summary>
    public static class SeqGuid
    {
        /// <summary>
        ///     Generates a COMB Guid which solves the fragmented index issue.
        ///     See: http://davybrion.com/blog/2009/05/using-the-guidcomb-identifier-strategy
        /// </summary>
        public static Guid Create()
        {
            var b = Guid.NewGuid().ToByteArray();
            var dateTime = new DateTime(1900, 1, 1);
            var now = DateTime.Now;
            var timeSpan = new TimeSpan(now.Ticks - dateTime.Ticks);
            var timeOfDay = now.TimeOfDay;
            var bytes1 = BitConverter.GetBytes(timeSpan.Days);
            var bytes2 = BitConverter.GetBytes((long) (timeOfDay.TotalMilliseconds / 3.333333));
            Array.Reverse(bytes1);
            Array.Reverse(bytes2);
            Array.Copy(bytes1, bytes1.Length - 2, b, b.Length - 6, 2);
            Array.Copy(bytes2, bytes2.Length - 4, b, b.Length - 4, 4);
            return new Guid(b);
        }
    }
}