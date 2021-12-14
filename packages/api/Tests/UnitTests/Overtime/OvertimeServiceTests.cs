using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Options;
using AlvTime.Business.Overtime;
using AlvTime.Business.TimeEntries;
using AlvTime.Business.Utils;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Tests.UnitTests.Overtime
{
    public class OvertimeServiceTests
    {
        private readonly AlvTime_dbContext context;
        private readonly IOptionsMonitor<TimeEntryOptions> options;
        private readonly Mock<IUserContext> userContextMock;

        public OvertimeServiceTests()
        {
            context = new AlvTimeDbContextBuilder()
                .WithTasks()
                .WithLeaveTasks()
                .WithProjects()
                .WithUsers()
                .WithCustomers()
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
            
            userContextMock = new Mock<IUserContext>();

            var user = new AlvTime.Business.Models.User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone"
            };

            userContextMock.Setup(context => context.GetCurrentUser()).Returns(user);
        }
        
        [Fact]
        public void GetEarnedOvertime_WorkedRegularDay_NoOvertime()
        {
            var dateToTest = new DateTime(2021, 12, 13); //Monday
            var timeEntryService = CreateTimeEntryService();
            var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 7.5M, 1); //Monday
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId} });
            
            var overtimeService = CreateOvertimeService();
            var earnedOvertime = overtimeService.GetEarnedOvertime(new OvertimeQueryFilter
                { StartDate = dateToTest, EndDate = dateToTest });
            Assert.Empty(earnedOvertime);
        }
        
        [Fact]
        public void GetEarnedOvertime_Worked9AndAHalfHoursOnWeekday_2HoursOvertime()
        {
            var dateToTest = new DateTime(2021, 12, 13); //Monday
            var timeEntryService = CreateTimeEntryService();
            var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 9.5M, 1); //Monday
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId} });

            var overtimeService = CreateOvertimeService();
            var earnedOvertime = overtimeService.GetEarnedOvertime(new OvertimeQueryFilter
                { StartDate = dateToTest, EndDate = dateToTest });
            
            Assert.Single(earnedOvertime);
            Assert.Equal(2, earnedOvertime.First().Value);
            Assert.Equal(1.0M, earnedOvertime.First().CompensationRate);
            Assert.Equal(1, earnedOvertime.First().UserId);
            Assert.Equal(dateToTest, earnedOvertime.First().Date);
        }
        
        [Fact]
        public void GetEarnedOvertime_WorkedOvertimeWithDifferentCompRatesOnWeekday_CorrectOvertimeWithCompRates()
        {
            var dateToTest = new DateTime(2021, 12, 13); //Monday
            var timeEntryService = CreateTimeEntryService();
            var timeEntry1 = CreateTimeEntryWithCompensationRate(dateToTest, 9.5M, 1.5M, out int taskId1); 
            var timeEntry2 = CreateTimeEntryWithCompensationRate(dateToTest, 1M, 1.0M, out int taskId2); 
            var timeEntry3 = CreateTimeEntryWithCompensationRate(dateToTest, 1M, 0.5M, out int taskId3); 
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId} });
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId} });

            var overtimeService = CreateOvertimeService();
            var earnedOvertime = overtimeService.GetEarnedOvertime(new OvertimeQueryFilter
                { StartDate = dateToTest, EndDate = dateToTest });
            
            Assert.Equal(3, earnedOvertime.Count);
            Assert.Equal(4, earnedOvertime.Sum(ot => ot.Value));
        }
        
        [Fact]
        public void GetEarnedOvertime_Worked2HoursOnASaturday_2HoursOvertime()
        {
            var dateToTest = new DateTime(2021, 12, 11); //Saturday
            var timeEntryService = CreateTimeEntryService();
            var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 2M, 1);
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId} });

            var overtimeService = CreateOvertimeService();
            var earnedOvertime = overtimeService.GetEarnedOvertime(new OvertimeQueryFilter
                {StartDate = dateToTest, EndDate = dateToTest });
            
            Assert.Single(earnedOvertime);
            Assert.Equal(2, earnedOvertime.Sum(ot => ot.Value));
            Assert.Equal(1.0M, earnedOvertime.First().CompensationRate);
        }
        
        [Fact]
        public void GetEarnedOvertime_Worked2HoursOnARedDay_2HoursOvertime()
        {
            var dateToTest = new DateTime(2021, 04, 01); //Skjaertorsdag
            var timeEntryService = CreateTimeEntryService();
            var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 2M, 1);
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId} });

            var overtimeService = CreateOvertimeService();
            var earnedOvertime = overtimeService.GetEarnedOvertime(new OvertimeQueryFilter
                { StartDate = dateToTest, EndDate = dateToTest });
            
            Assert.Single(earnedOvertime);
            Assert.Equal(2, earnedOvertime.Sum(ot => ot.Value));
            Assert.Equal(1.0M, earnedOvertime.First().CompensationRate);
        }
        
        [Fact]
        public void GetEarnedOvertime_RegisteredVacationOnSaturday_NoOvertime()
        {
            var dateToTest = new DateTime(2021, 12, 11); //Saturday
            var timeEntryService = CreateTimeEntryService();
            var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 7.5M, 14);
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId} });

            var overtimeService = CreateOvertimeService();
            var earnedOvertime = overtimeService.GetEarnedOvertime(new OvertimeQueryFilter
                { StartDate = dateToTest, EndDate = dateToTest });
            
            Assert.Empty(earnedOvertime);
        }
        
        [Fact]
        public void GetEarnedOvertime_Registered10HoursVacationOnWeekday_NoOvertime()
        {
            var dateToTest = new DateTime(2021, 12, 13); //Monday
            var timeEntryService = CreateTimeEntryService();
            var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 10M, 14);
            Assert.Throws<Exception>(() => timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId} }));
        }
        
        [Fact]
        public void GetEarnedOvertime_WorkedOvertimeWithDifferentCompRatesOnWeekdayThenChangedEntries_CorrectOvertimeWithCompRates()
        {
            var dateToTest = new DateTime(2021, 12, 13); //Monday
            var timeEntryService = CreateTimeEntryService();
            var timeEntry1 = CreateTimeEntryWithCompensationRate(dateToTest, 9.5M, 1.5M, out int taskId1); 
            var timeEntry2 = CreateTimeEntryWithCompensationRate(dateToTest, 1M, 1.0M, out int taskId2); 
            var timeEntry3 = CreateTimeEntryWithCompensationRate(dateToTest, 1M, 0.5M, out int taskId3); 
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId} });
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId} });

            var overtimeService = CreateOvertimeService();
            var earnedOvertimeOriginal = overtimeService.GetEarnedOvertime(new OvertimeQueryFilter
                { StartDate = dateToTest, EndDate = dateToTest });
            
            var timeEntry4 = CreateTimeEntryForExistingTask(dateToTest, 8M, taskId1);
            var timeEntry5 = CreateTimeEntryForExistingTask(dateToTest, 1.5M, taskId3); 
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry4.Date, Value = timeEntry4.Value, TaskId = timeEntry4.TaskId } });
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry5.Date, Value = timeEntry5.Value, TaskId = timeEntry5.TaskId} });
            
            var earnedOvertimeUpdated = overtimeService.GetEarnedOvertime(new OvertimeQueryFilter
                { StartDate = dateToTest, EndDate = dateToTest });
            
            Assert.NotEqual(earnedOvertimeOriginal, earnedOvertimeUpdated);
            Assert.Equal(3, earnedOvertimeOriginal.Count);
            Assert.Equal(3, earnedOvertimeUpdated.Count);
            Assert.Equal(3, earnedOvertimeUpdated.Sum(ot => ot.Value));
        }
        
        [Fact]
        public void GetEarnedOvertime_WorkedOvertimeOverSeveralDaysAndChangedOneDay_CorrectOvertimeWithCompRates()
        {
            var timeEntryService = CreateTimeEntryService();
            var timeEntry1 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 9.5M, 1.5M, out int taskId1); //Monday 
            var timeEntry2 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 8M, 1.0M, out int taskId2); //Tuesday
            var timeEntry3 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 8), 11M, 0.5M, out int taskId3); //Wednesday
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId} });
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId} });

            var timeEntry4 = CreateTimeEntryForExistingTask(new DateTime(2021, 12, 7), 8.5M, taskId2); 
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry4.Date, Value = timeEntry4.Value, TaskId = timeEntry4.TaskId } });
            
            var overtimeService = CreateOvertimeService();
            var earnedOvertime = overtimeService.GetEarnedOvertime(new OvertimeQueryFilter
                { StartDate = new DateTime(2021, 12, 6), EndDate = new DateTime(2021, 12, 8) });
            
            Assert.Equal(3, earnedOvertime.Count);
            Assert.Equal(6.5M, earnedOvertime.Sum(ot => ot.Value));
        }

        [Fact]
        public void GetAvailableHours_Worked9AndAHalfHoursWith1AndAHalfCompRate_2HoursBeforeComp3HoursAfterComp()
        {
            var timeEntryService = CreateTimeEntryService();
            var timeEntry1 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 9.5M, 1.5M, out int taskId1); //Monday
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>{ new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

            var overtimeService = CreateOvertimeService();
            var availableHours = overtimeService.GetAvailableOvertimeHours();
            
            Assert.Single(availableHours.Entries);
            Assert.Equal(2, availableHours.AvailableHoursBeforeCompensation);
            Assert.Equal(3, availableHours.AvailableHoursAfterCompensation);
        }

        private TimeEntryService CreateTimeEntryService()
        {
            return new TimeEntryService(CreateTimeEntryStorage(), null, options, userContextMock.Object, CreateTaskUtils());
        }

        private TimeEntryStorage CreateTimeEntryStorage()
        {
            return new TimeEntryStorage(context, CreateOvertimeService());
        }

        private OvertimeService CreateOvertimeService()
        {
            return new OvertimeService(new OvertimeStorage(context), userContextMock.Object, new TaskStorage(context), options, CreateTaskUtils());
        }

        private TaskUtils CreateTaskUtils()
        {
            return new TaskUtils(new TaskStorage(context), options);
        }

        private static Hours CreateTimeEntryForExistingTask(DateTime date, decimal value, int taskId)
        {
            return new Hours
            {
                User = 1,
                Date = date,
                Value = value,
                TaskId = taskId
            };
        }
        
        private Hours CreateTimeEntryWithCompensationRate(DateTime date, decimal value, decimal compensationRate, out int taskId)
        {
            taskId = new Random().Next(1000, 10000000);
            var task = new Task { Id = taskId, Project = 1, };
            context.Task.Add(task);
            context.CompensationRate.Add(new CompensationRate { TaskId = taskId, Value = compensationRate, FromDate = new DateTime(2021, 01, 01)});
            context.SaveChanges();
            
            return new Hours
            {
                User = 1,
                Date = date,
                Value = value,
                Task = task,
                TaskId = taskId
            };
        }
    }
}