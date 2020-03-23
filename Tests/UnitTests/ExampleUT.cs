using System;
using Xunit;

namespace Tests
{
    public class ExampleUT
    {
        [Fact]
        public void Test1()
        {
            string s1 = "hel";
            string s2 = "lo";
            string s3 = s1 + s2;

            Assert.Equal("hello", s3);
        }
    }
}
