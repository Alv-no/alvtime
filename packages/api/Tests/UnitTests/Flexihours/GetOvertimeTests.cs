using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using AlvTime.Business.FlexiHours;
using Xunit;
using Microsoft.Extensions.Options;
using AlvTime.Business.Options;
using AlvTime.Persistence.EconomyDataDBModels;
using AlvTime.Persistence.Repositories.AlvEconomyData;

namespace Tests.UnitTests.Flexihours
{
    public class GetOvertimeTests
    {
        private readonly AlvTime_dbContext _context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

        private AlvEconomyDataContext _economyDataContext = new AlvEconomyDataDbContextBuilder().WithEmployeeSalary().CreateDbContext();

        private readonly DateTime _startDate = new DateTime(2021, 01, 05);
        private readonly DateTime _endDate = DateTime.Now.Date;

        [Fact]
        public void GetFlexhours_NoWorkAtAll_AvailableIs0Overtime0Flex()
        {
            FlexhourStorage flexhourStorage = CreateStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhours_NormalWorkday_NoFlexhour()
        {
            _context.Hours.Add(CreateTimeEntry(date: _startDate, value: 7.5M, out int taskid));
            _context.SaveChanges();

            FlexhourStorage flexhourStorage = CreateStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhours_WorkedOvertime_PositiveFlexAndOvertime()
        {
            _context.Hours.Add(CreateTimeEntry(date: _startDate, value: 10M, out int taskid));
            _context.SaveChanges();

            var flexhourStorage = CreateStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(2.5M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(2.5M, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhours_ExhangedHoursIntoPayout_AvailableHoursAreCompensatedForPayout()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 01, 06), value: 15M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 2.0M));
            _context.SaveChanges();

            var flexhourStorage = CreateStorage();
            flexhourStorage.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2026, 01, 06),
                Hours = 3M
            }, userId: 1);

            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(4.5M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(9M, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhours_WorkedMultipleDays_FlexForMultipleDaysAreSummed()
        {
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 01, 06), value: 10M, out int taskid));
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 01, 07), value: 10M, out int taskid2));
            _context.SaveChanges();

            var flexhourStorage = CreateStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(5, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(5, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhours_MultipleEmployees_FlexForSpecifiedEmployeeIsCalculated()
        {
            var entry1 = CreateTimeEntry(date: _startDate, value: 10M, out int taskid);
            entry1.User = 1;
            _context.Hours.Add(entry1);

            var entry2 = CreateTimeEntry(date: _startDate, value: 8M, out int taskid2);
            entry2.User = 2;
            _context.Hours.Add(entry2);
            _context.SaveChanges();

            var flexhourStorage = CreateStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(2.5M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(2.5M, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhoursToday_EntriesBeforeStartDate_NotTakenIntoAccount()
        {
            _context.Hours.Add(CreateTimeEntry(date: _startDate, value: 10M, out int taskid));
            _context.Hours.Add(CreateTimeEntry(date: _startDate.AddDays(-1), value: 10M, out int taskid2));
            _context.SaveChanges();

            var flexhourStorage = CreateStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(2.5M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(2.5M, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhours_NotWorkedInWeekend_NoImpactOnOverTimeNorFlex()
        {
            // saturday and sunday:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 04), value: 0M, out _));
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 05), value: 0M, out _));
            _context.SaveChanges();

            var flexHourStorage = CreateStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void GetEntriesbyDate_MultipleTasks_FlexhoursCompensatesForMultipleTasks()
        {
            _context.Hours.Add(CreateTimeEntry(date: _startDate, value: 5M, out _));
            _context.Hours.Add(CreateTimeEntry(date: _startDate, value: 2.5M, out _));
            _context.SaveChanges();

            var flexHourStorage = CreateStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexedhours_FlexingMoreThanAvailable_CannotFlex()
        {
            _context.Hours.Add(CreateTimeEntry(date: _startDate, value: 8.5M, out int taskid));
            _context.Hours.Add(CreateFlexEntry(date: _startDate.AddDays(1), value: 2M));
            _context.SaveChanges();

            var flexhourStorage = CreateStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexedhours_FlexingBeforeRecordedHours_CannotFlex()
        {
            _context.Hours.Add(CreateFlexEntry(date: _startDate, value: 2M));
            _context.SaveChanges();

            var flexHourStorage = CreateStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexedhours_RecordingHoursBeforeStartOfOvertimeSystem_NoOvertime()
        {
            var user = _context.User.First();
            user.StartDate = new DateTime(2020, 11, 01);

            _context.Hours.Add(CreateTimeEntry(new DateTime(2020, 11, 02), 9, out int taskId));
            _context.SaveChanges();

            var flexHourStorage = CreateStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexedhours_RecordedVacationOnRedDay_NoFlexHours()
        {
            var user = _context.User.First();

            _context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2021, 01, 01),
                Value = 7.5M,
                Task = new Task { Id = 13 }
            });
            _context.SaveChanges();

            var flexHourStorage = CreateStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexedhours_RecordedVacationOnWeekend_NoFlexHours()
        {
            var user = _context.User.First();

            _context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2021, 02, 07),
                Value = 7.5M,
                Task = new Task { Id = 13 }
            });
            _context.SaveChanges();

            var flexHourStorage = CreateStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        private FlexhourStorage CreateStorage()
        {
            return new FlexhourStorage(new TimeEntryStorage(_context), _context, new TestTimeEntryOptions(
                new TimeEntryOptions { 
                    FlexTask = 18,
                    PaidHolidayTask = 13,
                    UnpaidHolidayTask = 19,
                    ReportUser = 11, 
                    StartOfOvertimeSystem = new DateTime(2021, 01, 01) 
                }),  
                
                new OvertimePayoutStorage(_economyDataContext, new EmployeeHourlySalaryStorage(_economyDataContext)));
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

        private static Hours CreateFlexEntry(DateTime date, decimal value)
        {
            return new Hours
            {
                Date = date,
                Task = new Task { Id = 18 },
                TaskId = 18,
                User = 1,
                Value = value
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
}
