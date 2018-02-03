using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FutureState.Flow.Tests
{
    public class TestFailure
    {
        public void AlwaysFail()
        {
            Assert.False(true);
        }
    }
}
