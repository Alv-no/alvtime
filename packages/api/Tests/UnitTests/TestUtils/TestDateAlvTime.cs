using System;
using AlvTime.Business.Utils;

namespace Tests.UnitTests.TestUtils;

public class TestDateAlvTimeProvider : IDateAlvTimeProvider
{
    public DateTime OverriddenValue = DateTime.Now;

    public DateTime Now => OverriddenValue;
}