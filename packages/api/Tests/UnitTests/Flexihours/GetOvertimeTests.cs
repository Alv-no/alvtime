using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Interfaces;
using Xunit;
using Microsoft.Extensions.Options;
using AlvTime.Business.Options;
using AlvTime.Business.Overtime;
using Moq;

namespace Tests.UnitTests.Flexihours
{
    public class GetOvertimeTests
    {
        private readonly AlvTime_dbContext context;
        private readonly IOptionsMonitor<TimeEntryOptions> options;

        public GetOvertimeTests()
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
        
        private readonly DateTime _startDate = new DateTime(2021, 01, 05);
        private readonly DateTime _endDate = DateTime.Now.Date;

        [Fact]
        public void GetFlexhours_NoWorkAtAll_AvailableIs0Overtime0Flex()
        {
            FlexhourStorage flexhourStorage = CreateFlexhourStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhours_NormalWorkday_NoFlexhour()
        {
            context.Hours.Add(CreateTimeEntry(date: _startDate, value: 7.5M, out int taskid));
            context.SaveChanges();

            FlexhourStorage flexhourStorage = CreateFlexhourStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhours_WorkedOvertime_PositiveFlexAndOvertime()
        {
            context.Hours.Add(CreateTimeEntry(date: _startDate, value: 10M, out int taskid));
            context.SaveChanges();

            var flexhourStorage = CreateFlexhourStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(2.5M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(2.5M, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhours_ExhangedHoursIntoPayout_AvailableHoursAreCompensatedForPayout()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 01, 06), value: 15M, out int taskid));
            context.CompensationRate.Add(CreateCompensationRate(taskid, 2.0M));
            context.SaveChanges();

            var flexhourStorage = CreateFlexhourStorage();
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
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 01, 06), value: 10M, out int taskid));
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 01, 07), value: 10M, out int taskid2));
            context.SaveChanges();

            var flexhourStorage = CreateFlexhourStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(5, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(5, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhours_MultipleEmployees_FlexForSpecifiedEmployeeIsCalculated()
        {
            var entry1 = CreateTimeEntry(date: _startDate, value: 10M, out int taskid);
            entry1.User = 1;
            context.Hours.Add(entry1);

            var entry2 = CreateTimeEntry(date: _startDate, value: 8M, out int taskid2);
            entry2.User = 2;
            context.Hours.Add(entry2);
            context.SaveChanges();

            var flexhourStorage = CreateFlexhourStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(2.5M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(2.5M, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhoursToday_EntriesBeforeStartDate_NotTakenIntoAccount()
        {
            context.Hours.Add(CreateTimeEntry(date: _startDate, value: 10M, out int taskid));
            context.Hours.Add(CreateTimeEntry(date: _startDate.AddDays(-1), value: 10M, out int taskid2));
            context.SaveChanges();

            var flexhourStorage = CreateFlexhourStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(2.5M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(2.5M, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhours_NotWorkedInWeekend_NoImpactOnOverTimeNorFlex()
        {
            // saturday and sunday:
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 04), value: 0M, out _));
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 05), value: 0M, out _));
            context.SaveChanges();

            var flexHourStorage = CreateFlexhourStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void GetEntriesbyDate_MultipleTasks_FlexhoursCompensatesForMultipleTasks()
        {
            context.Hours.Add(CreateTimeEntry(date: _startDate, value: 5M, out _));
            context.Hours.Add(CreateTimeEntry(date: _startDate, value: 2.5M, out _));
            context.SaveChanges();

            var flexHourStorage = CreateFlexhourStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexedhours_FlexingMoreThanAvailable_CannotFlex()
        {
            context.Hours.Add(CreateTimeEntry(date: _startDate, value: 8.5M, out int taskid));
            context.Hours.Add(CreateFlexEntry(date: _startDate.AddDays(1), value: 2M));
            context.SaveChanges();

            var flexhourStorage = CreateFlexhourStorage();
            var flexhours = flexhourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexedhours_FlexingBeforeRecordedHours_CannotFlex()
        { 
            context.Hours.Add(CreateFlexEntry(date: _startDate, value: 2M));
            context.SaveChanges();

            var flexHourStorage = CreateFlexhourStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexedhours_RecordingHoursBeforeStartOfOvertimeSystem_NoOvertime()
        {
            var user = context.User.First();
            user.StartDate = new DateTime(2020, 11, 01);

            context.Hours.Add(CreateTimeEntry(new DateTime(2020, 11, 02), 9, out int taskId));
            context.SaveChanges();

            var flexHourStorage = CreateFlexhourStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexedhours_RecordedVacationOnRedDay_NoFlexHours()
        {
            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2021, 01, 01),
                Value = 7.5M,
                Task = new Task { Id = 13, Project = 9 }
            });
            context.SaveChanges();

            var flexHourStorage = CreateFlexhourStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexedhours_RecordedVacationOnWeekend_NoFlexHours()
        {
            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2021, 02, 07),
                Value = 7.5M,
                Task = new Task { Id = 13, Project = 9 }
            });
            context.SaveChanges();

            var flexHourStorage = CreateFlexhourStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }
        
        [Fact]
        public void GetAvailableHours_RecordedAlvDayTaskOnAlvDay_NoFlexHours()
        {
            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2021, 03, 29),
                Value = 7.5M,
                Task = new Task { Id = 20, Project = 9 }
            });
            context.SaveChanges();

            var flexHourStorage = CreateFlexhourStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(0M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(0, flexhours.AvailableHoursAfterCompensation);
        }
                
        [Fact]
        public void GetAvailableHours_Worked5HoursOnWeekend_5HoursOvertimeBeforeComp6AfterComp()
        {
            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2021, 08, 08),
                Value = 4M,
                Task = new Task { Id = 1, Project = 1 }
            });
            context.CompensationRate.Add(new CompensationRate
                { FromDate = new DateTime(2020, 01, 01), TaskId = 1, Value = 1.5M });
            context.SaveChanges();

            var flexHourStorage = CreateFlexhourStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(4M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(6M, flexhours.AvailableHoursAfterCompensation);
        }
                
        [Fact]
        public void GetAvailableHours_RecordedNonAbsenceOnAlvDay_GetsCorrectOvertime()
        {
            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2021, 03, 29),
                Value = 5M,
                Task = new Task { Id = 1, Project = 1}
            });
            context.CompensationRate.Add(new CompensationRate { TaskId = 1, Value = 1.5M, FromDate = new DateTime(2020, 01, 01)});
            context.SaveChanges();

            var flexHourStorage = CreateFlexhourStorage();
            var flexhours = flexHourStorage.GetAvailableHours(1, _startDate, _endDate);

            Assert.Equal(5M, flexhours.AvailableHoursBeforeCompensation);
            Assert.Equal(7.5M, flexhours.AvailableHoursAfterCompensation);
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

        private static Hours CreateFlexEntry(DateTime date, decimal value)
        {
            return new Hours
            {
                Date = date,
                Task = new Task { Id = 18, Project = 9 },
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
    }
}
