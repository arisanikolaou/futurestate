using FutureState.Reflection;
using System.Linq;
using Xunit;

namespace FutureState.Autofac.Tests
{
    public class AppTypeScannerTests
    {
        [Fact]
        public void ApplicationCanScanDomainTypes()
        {
            var subject = AppTypeScanner.Default;
            var domainTypes = subject.GetAppDomainTypes().Select(m => m.Name).ToList();

            Assert.False(domainTypes.Count == 0);

            Assert.Contains(typeof(PublicTestType).Name, domainTypes);
            Assert.DoesNotContain(typeof(PublicTestType2).Name, domainTypes);
        }
    }

    public class PublicTestType
    {
    }

    // should not be scanned
    internal class PublicTestType2
    {
    }
}