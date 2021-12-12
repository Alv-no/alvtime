using System;
using AlvTime.Business.Options;
using Microsoft.Extensions.Options;

namespace Tests.UnitTests
{
    public class TestTimeEntryOptions : IOptionsMonitor<TimeEntryOptions>
    {
        public TimeEntryOptions CurrentValue { get; }

        public TestTimeEntryOptions(TimeEntryOptions currentValue)
        {
            CurrentValue = currentValue;
        }

        public TimeEntryOptions Get(string name)
        {
            return CurrentValue;
        }

        public IDisposable OnChange(Action<TimeEntryOptions, string> listener)
        {
            throw new NotImplementedException();
        }
    }
}