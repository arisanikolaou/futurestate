using Xunit;

namespace FutureState.Common.Tests
{
    public class PropertyInvokerTests
    {
        public string Name { get; set; }

        [Fact]
        public void WhenInvokingPropertyGetterWillReturnValidValue()
        {
            var getter = GetType().GetProperty("Name").GetPropertyGetterFn();

            Name = "Value";

            var value = (string) getter(this);

            Assert.Equal("Value", value);
        }
    }
}