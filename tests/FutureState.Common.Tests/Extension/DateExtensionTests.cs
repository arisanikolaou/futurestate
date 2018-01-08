using System;
using Xunit;

namespace FutureState.Common.Tests.Extension
{
    public class DateExtensionTests
    {
        [Theory]
        [InlineData(1, 7, 30)]
        [InlineData(1, 1, 24)]
        public void DateTests(int startMonth, int endMonth, int expectedMonths)
        {
            DateTime start = new DateTime(2000, startMonth, 1);
            DateTime end = new DateTime(2002, endMonth, 1);

            Assert.Equal(expectedMonths, start.MonthDiff(end));
        }
    }
}
