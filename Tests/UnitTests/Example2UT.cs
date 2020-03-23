using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace Tests.UnitTests
{
    public class Example2UT
    {
        [Fact]
        public void Test3_Fails()
        {
            string s1 = "hel";
            string s2 = "lo";
            string s3 = s1 + s2;

            Assert.Equal("hello", s3);
        }
    }
}
