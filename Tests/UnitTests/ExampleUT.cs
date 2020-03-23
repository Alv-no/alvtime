using System;
using Xunit;

namespace Tests
{
    public class ExampleUT
    {
        [Fact]
        public void Test1_Passes()
        {
            string s1 = "hel";
            string s2 = "lo";
            string s3 = s1 + s2;

            Assert.Equal("hello", s3);
        }

        [Fact]
        public void Test2_Fails()
        {
            string s1 = "hel";
            string s2 = "lo";
            string s3 = s1 + s2;

            Assert.Equal("helo", s3);
        }

        [Fact]
        public void Test3_Fails()
        {
            string s1 = "hel";
            string s2 = "lo";
            string s3 = s1 + s2;

            Assert.Equal("helewo", s3);
        }
    }
}
