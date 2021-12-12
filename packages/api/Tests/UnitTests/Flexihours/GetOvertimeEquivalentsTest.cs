using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Overtime;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static Tests.UnitTests.Flexihours.GetOvertimeTests;

namespace Tests.UnitTests.Flexihours
{
    public class GetOvertimeEquivalentsTest
    {
        private readonly AlvTime_dbContext context;
        private readonly IOptionsMonitor<TimeEntryOptions> options;

        public GetOvertimeEquivalentsTest()
        {
            context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            var entryOptions = new TimeEntryOptions
            {
                SickDaysTask = 14,
                PaidHolidayTask = 13,
                UnpaidHolidayTask = 19,
                FlexTask = 18,
                StartOfOvertimeSystem = new DateTime(2020, 01, 01),
                AbsenceProject = 9
            };
            options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);
        }

        private readonly DateTime _startDate = new(2020, 01, 02);
        private readonly DateTime _endDate = DateTime.Now.Date;

        [Fact]
        public void GetOvertime_Worked7AndAHalfHours_NoOvertime()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 7.5M, out int taskId));
            context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.0M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(0, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked10HoursDay1And5HoursDay2NoFlexRecorded_2AndAHalfOvertime()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M, out int taskId));
            context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.0M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 5M, out int taskId2));
            context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.0M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(2.5M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked5HoursBillableAnd5Hours0Point5CompRate_1Point25Overtime()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId));
            context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.5M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId2));
            context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 0.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(1.25M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked9P5HoursBillableAnd1Point5CompRate_1Point25Overtime()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 9.5M, out int taskId));
            context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(3M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked5Hours0Point5CompRateAnd5HoursBillableAnd_1Point25Overtime()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId));
            context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 0.5M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId2));
            context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(1.25M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked5Hours0Point5CompRateAnd7P5HoursBillableAnd5HoursAlvFredag_1Point25Overtime()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId));
            context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 0.5M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId2));
            context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.0M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 7.5M, out int taskId3 ));
            context.CompensationRate.Add(CreateCompensationRate(taskId3, compRate: 1.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(7.5M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked10Hours0Point5CompRateAnd10HoursBillableAnd10HoursAlvFredag_7P5Overtime()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M, out int taskId));
            context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 0.5M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 10M, out int taskId2));
            context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.0M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 10M, out int taskId3));
            context.CompensationRate.Add(CreateCompensationRate(taskId3, compRate: 1.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(7.5M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked5Hours0Point5CompRateAnd5HoursBillable_1P25Overtime()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId));
            context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 0.5M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 5M, out int taskId2));
            context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(1.25M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_OvertimeAndTimeOff_0Overtime()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M, out int taskId));
            context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 2.0M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 5M, out int taskId2));
            context.Hours.Add(new Hours
            {
                Task = new Task { Id = 18, Project = 9 },
                User = 1,
                Date = new DateTime(2020, 01, 03),
                Value = 2.5M
            });
            context.CompensationRate.Add(CreateCompensationRate(18, compRate: 1.0M));
            context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.0M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(0M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_OvertimeAndRegisteredPayout_5OvertimeLeft()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 17.5M, out int taskId));
            context.CompensationRate.Add(CreateCompensationRate(taskId ,compRate: 1M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

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
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 17.5M, out int taskId));
            context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 07), value: 12.5M, out int taskId2));
            context.CompensationRate.Add(CreateCompensationRate(taskId2, compRate: 1.5M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 07), value: 9M, out int taskId3));
            context.CompensationRate.Add(CreateCompensationRate(taskId3, compRate: 0.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var result = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 09),
                Hours = 11
            }, 1);

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(15.5M, OTequivalents);
            Assert.Equal(6.5M, result.HoursAfterCompensation);
        }

        [Fact]
        public void GetOvertime_NotRecordedBeforeStarting_5Overtime()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 04, 01), value: 12.5M, out int taskId));
            context.CompensationRate.Add(CreateCompensationRate(taskId, compRate: 1M));

            context.User.First().StartDate = new DateTime(2020, 04, 01);

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(5M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_FlexingBeforeWorkingWithHighCompRate_WillNotSpendHighCompRateWhenFlexing()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 8.5M, out int taskWithNormalCompensation));
            context.CompensationRate.Add(CreateCompensationRate(taskWithNormalCompensation, compRate: 1M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 07), value: 6.5M, out int sometask));
            context.CompensationRate.Add(CreateCompensationRate(sometask, compRate: 1M));

            context.Hours.Add(new Hours
            {
                Date = new DateTime(2020, 01, 07),
                Task = new Task
                {
                    Id = 18,
                    Project = 9
                },
                TaskId = 18,
                User = 1,
                Value = 1M
            });

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 08), value: 8.5M, out int taskWith5TimesCompensation));
            context.CompensationRate.Add(CreateCompensationRate(taskWith5TimesCompensation, compRate: 6M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(6M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked2HoursOnKristiHimmelfart_3HoursInOvertime()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 05, 21), value: 2M, out int taskWithNormalCompensation));
            context.CompensationRate.Add(CreateCompensationRate(taskWithNormalCompensation, compRate: 1.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(3M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked2HoursOnKristiHimmelfartAndMay17_6HoursInOvertime()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 05, 17), value: 2M, out int taskWithNormalCompensation2));
            context.CompensationRate.Add(CreateCompensationRate(taskWithNormalCompensation2, compRate: 1.5M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 05, 21), value: 2M, out int taskWithNormalCompensation));
            context.CompensationRate.Add(CreateCompensationRate(taskWithNormalCompensation, compRate: 1.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(6M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_Worked10HoursOnWorkdayAnd1HourWeekend()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 10M, out int taskWithNormalCompensation2));
            context.CompensationRate.Add(CreateCompensationRate(taskWithNormalCompensation2, compRate: 2M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 04), value: 4M, out int taskWithNormalCompensation));
            context.CompensationRate.Add(CreateCompensationRate(taskWithNormalCompensation, compRate: 0.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var OTequivalents = calculator.GetAvailableHours(1, _startDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(7M, OTequivalents);
        }

        [Fact]
        public void GetOvertime_1May2019AndSecondPinseDag2020_6HoursOvertime()
        {
            var user = context.User.First();
            user.StartDate = new DateTime(2019, 01, 01);
            context.SaveChanges();

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2019, 05, 01), value: 2M, out int taskWithCompensation2));
            context.CompensationRate.Add(CreateCompensationRate(taskWithCompensation2, compRate: 2M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 01), value: 4M, out int taskWithCompensation));
            context.CompensationRate.Add(CreateCompensationRate(taskWithCompensation, compRate: 0.5M));

            context.SaveChanges();

            FlexhourStorage calculator = new FlexhourStorage(CreateTimeEntryStorage(), context, new TestTimeEntryOptions(
                new TimeEntryOptions
                {
                    FlexTask = 18,
                    ReportUser = 11,
                    AbsenceProject = 9,
                    StartOfOvertimeSystem = new DateTime(2019, 01, 01)
                }));

            var OTequivalents = calculator.GetAvailableHours(1, user.StartDate, _endDate).AvailableHoursAfterCompensation;
            Assert.Equal(6M, OTequivalents);
        }

        private FlexhourStorage CreateFlexhourStorage()
        {
            return new FlexhourStorage(CreateTimeEntryStorage(), context, options);
        }
        
        public TimeEntryStorage CreateTimeEntryStorage()
        {
            return new TimeEntryStorage(context, CreateOvertimeService());
        }

        public OvertimeService CreateOvertimeService()
        {
            var mockUserContext = new Mock<IUserContext>();

            var user = new AlvTime.Business.Models.User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone"
            };

            mockUserContext.Setup(context => context.GetCurrentUser()).Returns(user);

            return new OvertimeService(new OvertimeStorage(context), mockUserContext.Object, new TaskStorage(context), options);
        }

        private static Hours CreateTimeEntry(DateTime date, decimal value, out int taskId)
        {
            taskId = new Random().Next();

            return new Hours
            {
                User = 1,
                Date = date,
                Value = value,
                Task = new Task { Id = taskId, Project = 1 }
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
