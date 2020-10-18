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
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 7.5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
            Assert.Equal(0, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked10HoursDay1And5HoursDay2_0Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents(new DateTime(2020, 01, 01), new DateTime(2020, 01, 02), 1);
            Assert.Equal(0M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked5HoursBillableAnd5Hours0Point5CompRate_1Point25Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 5M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 0.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
            Assert.Equal(1.25M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked9P5HoursBillableAnd1Point5CompRate_1Point25Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 9.5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
            Assert.Equal(3M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked5Hours0Point5CompRateAnd5HoursBillableAnd_1Point25Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 0.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 5M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
            Assert.Equal(1.25M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked5Hours0Point5CompRateAnd7P5HoursBillableAnd5HoursAlvFredag_1Point25Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 0.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 5M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 7.5M, out int taskId3 ));
            _context.CompensationRate.Add(CreateCompensationRate(taskId3, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
            Assert.Equal(7.5M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked10Hours0Point5CompRateAnd10HoursBillableAnd10HoursAlvFredag_7P5Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 10M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 0.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 10M, out int taskId3));
            _context.CompensationRate.Add(CreateCompensationRate(taskId3, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents(new DateTime(2020, 01, 01), new DateTime(2020, 01, 03), 1);
            Assert.Equal(7.5M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked5Hours0Point5CompRateAnd5HoursBillable_1P25Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 0.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 01), value: 5M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);
            Assert.Equal(1.25M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_OvertimeAndTimeOff_6P75Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 10, 12), value: 10M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 2.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 10, 13), value: 5M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.0M));


            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents(new DateTime(2020, 10, 12), new DateTime(2020, 10, 13), 1);
            Assert.Equal(0M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_OvertimeAndTimdwadwaeOff_6P75Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 10, 12), value: 15M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId ,compRate: 1M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents(new DateTime(2020, 10, 12), new DateTime(2020, 10, 13), 1);
            Assert.Equal(0M, OTequivalents);
        }

        private FlexhourStorage CreateStorage()
        {
            return new FlexhourStorage(new TimeEntryStorage(_context), _context);
        }

        private static Hours CreateTimeEntry(DateTime date, decimal value, out int taskId)
        {
            taskId = new Random().Next();

            return new Hours
            {
                User = 1,
                Date = date,
                Value = value,
                Task = new Task { Id = taskId }
            };
        }

        private static CompensationRate CreateCompensationRate(int taskId, decimal compRate)
        {
            return new CompensationRate
            {
                FromDate = DateTime.UtcNow,
                Value = compRate,
                TaskId = taskId
            };
        }
    }
}
