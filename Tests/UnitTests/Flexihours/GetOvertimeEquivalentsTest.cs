using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using Xunit;

namespace Tests.UnitTests.Flexihours
{
    public class GetOvertimeEquivalentsTest
    {
        private AlvTime_dbContext _context = new AlvTimeDbContextBuilder()
        .WithUsers()
        .CreateDbContext();

        [Fact]
        public void GetOvertime_Worked7AndAHalfHours_NoOvertime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 7.5M));
            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
            Assert.Equal(0, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked10Hours_5Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M));
            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
            Assert.Equal(5M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked10HoursDay1And5HoursDay2_2point5Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M));
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M));
            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents(new DateTime(2020, 01, 01), new DateTime(2020, 01, 02), 1);
            Assert.Equal(2.5M, OTequivalents);
        }

        private FlexhourStorage CreateStorage()
        {
            return new FlexhourStorage(new TimeEntryStorage(_context), _context);
        }

        private static Hours CreateTimeEntry(DateTime date, decimal value)
        {
            return new Hours
            {
                User = 1,
                Date = date,
                Value = value,
                Task = new Task { }
            };
        }
    }
}
