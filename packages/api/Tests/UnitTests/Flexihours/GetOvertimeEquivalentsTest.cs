using AlvTime.Business.FlexiHours;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
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
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 7.5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), 1).overtime;
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

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 01, 03), 1).overtime;
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

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), 1).overtime;
            Assert.Equal(1.25M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked9P5HoursBillableAnd1Point5CompRate_1Point25Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 9.5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), 1).overtime;
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

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), 1).overtime;
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

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), 1).overtime;
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

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 01, 06), 1).overtime;
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

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), 1).overtime;
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

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 01, 03), 1).overtime;
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

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 01, 02), 1).overtime;
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

            _context.PaidOvertime.Add(new PaidOvertime
            {
                Date = new DateTime(2020, 01, 09),
                User = 1,
                Value = 12
            });

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 01, 09), 1);
            Assert.Equal(10M, OTequivalents.overtime);
        }

        [Fact]
        public void GetOvertime_NotRecordedBeforeStarting_5Overtime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 04, 01), value: 12.5M, out int taskId));
            _context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1M));

            _context.User.First().StartDate = new DateTime(2020, 04, 01);

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 01), new DateTime(2020, 04, 01), 1).overtime;
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

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 06), new DateTime(2020, 01, 08), 1).overtime;
            Assert.Equal(6M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked2HoursOnKristiHimmelfart_3HoursInOvertime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 05, 21), value: 2M, out int taskWithNormalCompensation));
            _context.CompensationRate.Add(CreateCompensationRate(taskWithNormalCompensation, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 05, 21), 1).overtime;
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

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 05, 21), 1).overtime;
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

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2020, 01, 04), 1).overtime;
            Assert.Equal(7M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_WorkedPinse2021AndChristmas2022AndNewYearsDay2023_7AndAHalfHoursOvertime()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 05, 23), value: 2M, out int taskWithCompensation2));
            _context.CompensationRate.Add(CreateCompensationRate(taskWithCompensation2, compRate: 2M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2022, 12, 24), value: 4M, out int taskWithCompensation));
            _context.CompensationRate.Add(CreateCompensationRate(taskWithCompensation, compRate: 0.5M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2023, 01, 01), value: 1M, out int taskWithCompensation3));
            _context.CompensationRate.Add(CreateCompensationRate(taskWithCompensation3, compRate: 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = CreateStorage();

            var OTequivalents = calculator.GetOvertimeEquivalents2(new DateTime(2020, 01, 02), new DateTime(2023, 01, 01), 1).overtime;
            Assert.Equal(7.5M, OTequivalents);
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
