using AlvTime.Business.Utils;
using System;

namespace Tests.UnitTests.Utils
{
    public class TestDateAlvTimeProvider : IDateAlvTimeProvider
    {
        public DateTime OverridedValue = DateTime.Now;

        public DateTime Now { get { return OverridedValue; } }
    }
}
