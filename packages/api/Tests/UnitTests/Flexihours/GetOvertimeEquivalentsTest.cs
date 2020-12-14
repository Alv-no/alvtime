using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using Xunit;
using static Tests.UnitTests.Flexihours.GetOvertimeTests;

namespace Tests.UnitTests.Flexihours
{
    public class GetOvertimeEquivalentsTest
    {
        private AlvTime_dbContext _context = new AlvTimeDbContextBuilder()
        .WithUsers()
        .CreateDbContext();

        private readonly DateTime _startDate = new DateTime(2020, 01, 02);
        private readonly DateTime _endDate = DateTime.Now.Date;

        [Fact]
        public void GetOvertime_Worked7AndAHalfHours_NoOvertime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 7.5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(0, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked10HoursDay1And5HoursDay2NoFlexRecorded_2AndAHalfOvertime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 5M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(2.5M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked5HoursBillableAnd5Hours0Point5CompRate_1Point25Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 0.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(1.25M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked9P5HoursBillableAnd1Point5CompRate_1Point25Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 9.5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(3M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked5Hours0Point5CompRateAnd5HoursBillableAnd_1Point25Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 0.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(1.25M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked5Hours0Point5CompRateAnd7P5HoursBillableAnd5HoursAlvFredag_1Point25Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 0.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 7.5M, out int taskId3 ));
            _context.CompensationRate.Add(CreateCompensationRate(taskId3, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(7.5M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked10Hours0Point5CompRateAnd10HoursBillableAnd10HoursAlvFredag_7P5Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 0.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 10M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 10M, out int taskId3));
            _context.CompensationRate.Add(CreateCompensationRate(taskId3, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(7.5M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked5Hours0Point5CompRateAnd5HoursBillable_1P25Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 0.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(1.25M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_OvertimeAndTimeOff_0Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 2.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 5M, out int taskId2));
            _context.Hours.Add(new Hours
            {
                Task = new Task { Id = 18 },
                User = 1,
                Date = new DateTime(2020, 01, 03),
                Value = 2.5M
            });
            _context.CompensationRate.Add(CreateCompensationRate(18, compRate: 1.0M));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(0M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_OvertimeAndRegisteredPayout_5OvertimeLeft()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 17.5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId ,compRate: 1M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 5
            }, 1);

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(5M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_OvertimeAndRegisteredPayoutVariousCompRates_10OvertimeLeft()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 17.5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 07), value: 12.5M, out int taskId2));
            _context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 07), value: 9M, out int taskId3));
            _context.CompensationRate.Add(CreateCompensationRate(taskId3, compRate: 0.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var result = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 09),
                Hours = 11
            }, 1).Value as PaidOvertime;

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(15.5M, OTequivalents);
            Assert.Equal(6.5M, result.HoursAfterCompRate);
        }

        [Fact]
        public void GetOvertime_NotRecordedBeforeStarting_5Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 04, 01), value: 12.5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1M));

            _context.User.First().StartDate = new DateTime(2020, 04, 01);

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(5M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_FlexingBeforeWorkingWithHighCompRate_WillNotSpendHighCompRateWhenFlexing()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 8.5M, out int taskWithNormalCompensation));
            _context.CompensationRate.Add(CreateCompensationRate(taskWithNormalCompensation, compRate: 1M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 07), value: 6.5M, out int sometask));
            _context.CompensationRate.Add(CreateCompensationRate(sometask, compRate: 1M));

            _context.Hours.Add(new Hours
            {
                Date = new DateTime(2020, 01, 07),
                Task = new Task
                {
                    Id = 18,
                },
                TaskId = 18,
                User = 1,
                Value = 1M
            });

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 08), value: 8.5M, out int taskWith5TimesCompensation));
            _context.CompensationRate.Add(CreateCompensationRate(taskWith5TimesCompensation, compRate: 6M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(6M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked2HoursOnKristiHimmelfart_3HoursInOvertime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 05, 21), value: 2M, out int taskWithNormalCompensation));
            _context.CompensationRate.Add(CreateCompensationRate(taskWithNormalCompensation, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(3M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked2HoursOnKristiHimmelfartAndMay17_6HoursInOvertime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 05, 17), value: 2M, out int taskWithNormalCompensation2));
            _context.CompensationRate.Add(CreateCompensationRate(taskWithNormalCompensation2, compRate: 1.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 05, 21), value: 2M, out int taskWithNormalCompensation));
            _context.CompensationRate.Add(CreateCompensationRate(taskWithNormalCompensation, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(6M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked10HoursOnWorkdayAnd1HourWeekend()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 10M, out int taskWithNormalCompensation2));
            _context.CompensationRate.Add(CreateCompensationRate(taskWithNormalCompensation2, compRate: 2M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 04), value: 4M, out int taskWithNormalCompensation));
            _context.CompensationRate.Add(CreateCompensationRate(taskWithNormalCompensation, compRate: 0.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(7M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_1May2019AndSecondPinseDag2020_6HoursOvertime()
        {
            var user = _context.User.First();
            user.StartDate = new DateTime(2019, 01, 01);
            _context.SaveChanges();

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2019, 05, 01), value: 2M, out int taskWithCompensation2));
            _context.CompensationRate.Add(CreateCompensationRate(taskWithCompensation2, compRate: 2M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 01), value: 4M, out int taskWithCompensation));
            _context.CompensationRate.Add(CreateCompensationRate(taskWithCompensation, compRate: 0.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = new FlexhourStorage(new TimeEntryStorage(_context), _context, new TestTimeEntryOptions(
                new TimeEntryOptions
                {
                    FlexTask = 18,
                    ReportUser = 11,
                    StartOfOvertimeSystem = new DateTime(2019, 01, 01)
                }));

            var OTequivalents = calculator.GetAvailableHours(1, user.StartDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(6M, OTequivalents);
        }

        private FlexhourStorage CreateStorage()
        {
            return new FlexhourStorage(new TimeEntryStorage(_context), _context, new TestTimeEntryOptions(
                new TimeEntryOptions
                {
                    FlexTask = 18,
                    ReportUser = 11,
                    StartOfOvertimeSystem = new DateTime(2020, 01, 01)
                }));
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
