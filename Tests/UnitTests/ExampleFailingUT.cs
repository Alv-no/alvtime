using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests.UnitTests
{
    public class ExampleFailingUT
    {
        [Fact]
        public void Test2_fails()
        {
            var text1 = "hei";
            var text2 = "hallo";

            Assert.True(text1.Equals(text2));
        }
    }
}
