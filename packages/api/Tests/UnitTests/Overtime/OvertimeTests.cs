using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Options;
using AlvTime.Business.Payouts;
using AlvTime.Business.TimeEntries;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Utils;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Tests.UnitTests.Overtime
{
    public class TimeRegistrationServiceTests
    {
        private readonly AlvTime_dbContext _context;
        private readonly IOptionsMonitor<TimeEntryOptions> _options;
        private readonly Mock<IUserContext> _userContextMock;

        public TimeRegistrationServiceTests()
        {
            _context = new AlvTimeDbContextBuilder()
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
            _options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);

            _userContextMock = new Mock<IUserContext>();

            var user = new AlvTime.Business.Models.User
            {
                Id = 1,
                Email = "someone@alv.no",
                Name = "Someone"
            };

            _userContextMock.Setup(context => context.GetCurrentUser()).Returns(user);
        }

        [Fact]
        public void GetEarnedOvertime_WorkedRegularDay_NoOvertime()
        {
            var dateToTest = new DateTime(2021, 12, 13); //Monday
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 7.5M, 1); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}});

            var earnedOvertime = timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
                {StartDate = dateToTest, EndDate = dateToTest});
            Assert.Empty(earnedOvertime);
        }

        [Fact]
        public void GetEarnedOvertime_Worked9AndAHalfHoursOnWeekday_2HoursOvertime()
        {
            var dateToTest = new DateTime(2021, 12, 13); //Monday
            var timeRegistrationService = CreateTimeRegistrationService();

            var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 9.5M, 1); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}});

            var earnedOvertime = timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
                {StartDate = dateToTest, EndDate = dateToTest});

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
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 = CreateTimeEntryWithCompensationRate(dateToTest, 9.5M, 1.5M, out int taskId1);
            var timeEntry2 = CreateTimeEntryWithCompensationRate(dateToTest, 1M, 1.0M, out int taskId2);
            var timeEntry3 = CreateTimeEntryWithCompensationRate(dateToTest, 1M, 0.5M, out int taskId3);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId}});

            var earnedOvertime = timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
                {StartDate = dateToTest, EndDate = dateToTest});

            Assert.Equal(3, earnedOvertime.Count);
            Assert.Equal(4, earnedOvertime.Sum(ot => ot.Value));
        }

        [Fact]
        public void GetEarnedOvertime_Worked2HoursOnASaturday_2HoursOvertime()
        {
            var dateToTest = new DateTime(2021, 12, 11); //Saturday
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 2M, 1);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}});

            var earnedOvertime = timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
                {StartDate = dateToTest, EndDate = dateToTest});

            Assert.Single(earnedOvertime);
            Assert.Equal(2, earnedOvertime.Sum(ot => ot.Value));
            Assert.Equal(1.0M, earnedOvertime.First().CompensationRate);
        }

        [Fact]
        public void GetEarnedOvertime_Worked2HoursOnARedDay_2HoursOvertime()
        {
            var dateToTest = new DateTime(2021, 04, 01); //Skjaertorsdag
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 2M, 1);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}});

            var earnedOvertime = timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
                {StartDate = dateToTest, EndDate = dateToTest});

            Assert.Single(earnedOvertime);
            Assert.Equal(2, earnedOvertime.Sum(ot => ot.Value));
            Assert.Equal(1.0M, earnedOvertime.First().CompensationRate);
        }

        [Fact]
        public void GetEarnedOvertime_RegisteredVacationOnSaturday_NoOvertime()
        {
            var dateToTest = new DateTime(2021, 12, 11); //Saturday
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 7.5M, 14);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}});

            var earnedOvertime = timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
                {StartDate = dateToTest, EndDate = dateToTest});

            Assert.Empty(earnedOvertime);
        }

        [Fact]
        public void GetEarnedOvertime_Registered10HoursVacationOnWeekday_NoOvertime()
        {
            var dateToTest = new DateTime(2021, 12, 13); //Monday
            var timeEntryService = CreateTimeRegistrationService();
            var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 10M, 14);
            Assert.Throws<Exception>(() => timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}}));
        }

        [Fact]
        public void
            GetEarnedOvertime_WorkedOvertimeWithDifferentCompRatesOnWeekdayThenChangedEntries_CorrectOvertimeWithCompRates()
        {
            var dateToTest = new DateTime(2021, 12, 13); //Monday
            var timeEntryService = CreateTimeRegistrationService();
            var timeEntry1 = CreateTimeEntryWithCompensationRate(dateToTest, 9.5M, 1.5M, out int taskId1);
            var timeEntry2 = CreateTimeEntryWithCompensationRate(dateToTest, 1M, 1.0M, out int taskId2);
            var timeEntry3 = CreateTimeEntryWithCompensationRate(dateToTest, 1M, 0.5M, out int taskId3);
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId}});

            var timeRegistrationService = CreateTimeRegistrationService();
            var earnedOvertimeOriginal = timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
                {StartDate = dateToTest, EndDate = dateToTest});

            var timeEntry4 = CreateTimeEntryForExistingTask(dateToTest, 8M, taskId1);
            var timeEntry5 = CreateTimeEntryForExistingTask(dateToTest, 1.5M, taskId3);
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry4.Date, Value = timeEntry4.Value, TaskId = timeEntry4.TaskId}});
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry5.Date, Value = timeEntry5.Value, TaskId = timeEntry5.TaskId}});

            var earnedOvertimeUpdated = timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
                {StartDate = dateToTest, EndDate = dateToTest});

            Assert.NotEqual(earnedOvertimeOriginal, earnedOvertimeUpdated);
            Assert.Equal(3, earnedOvertimeOriginal.Count);
            Assert.Equal(3, earnedOvertimeUpdated.Count);
            Assert.Equal(3, earnedOvertimeUpdated.Sum(ot => ot.Value));
            Assert.Equal(4.5M, earnedOvertimeOriginal.Sum(ot => ot.Value * ot.CompensationRate));
            Assert.Equal(2.5M, earnedOvertimeUpdated.Sum(ot => ot.Value * ot.CompensationRate));
        }

        [Fact]
        public void GetEarnedOvertime_WorkedOvertimeOverSeveralDaysAndChangedOneDay_CorrectOvertimeWithCompRates()
        {
            var timeEntryService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 9.5M, 1.5M, out int taskId1); //Monday 
            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 8M, 1.0M, out int taskId2); //Tuesday
            var timeEntry3 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 8), 11M, 0.5M, out int taskId3); //Wednesday
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId}});

            var timeEntry4 = CreateTimeEntryForExistingTask(new DateTime(2021, 12, 7), 8.5M, taskId2);
            timeEntryService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry4.Date, Value = timeEntry4.Value, TaskId = timeEntry4.TaskId}});

            var timeRegistrationService = CreateTimeRegistrationService();
            var earnedOvertime = timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
                {StartDate = new DateTime(2021, 12, 6), EndDate = new DateTime(2021, 12, 8)});

            Assert.Equal(3, earnedOvertime.Count);
            Assert.Equal(6.5M, earnedOvertime.Sum(ot => ot.Value));
        }

        [Fact]
        public void GetAvailableHours_Worked9AndAHalfHoursWith1AndAHalfCompRate_2HoursBeforeComp3HoursAfterComp()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var dateToTest = new DateTime(2021, 12, 6);
            var timeEntry1 = CreateTimeEntryWithCompensationRate(dateToTest, 9.5M, 1.5M, out int taskId1); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Single(availableHours.Entries);
            Assert.Equal(2, availableHours.AvailableHoursBeforeCompensation);
            Assert.Equal(3, availableHours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetTimeEntries_UpdateOvertimeFails_NoHoursRegistered()
        {
            var sqliteContext = new AlvTimeDbContextBuilder(true)
                .WithCustomers()
                .WithProjects()
                .WithTasks()
                .WithLeaveTasks()
                .WithUsers()
                .CreateDbContext();
            var mockRepo = new Mock<TimeRegistrationStorage>(sqliteContext);
            mockRepo.Setup(mr => mr.DeleteOvertimeOnDate(It.IsAny<DateTime>(), It.IsAny<int>()))
                .Throws(new Exception());
            mockRepo.CallBase = true;
            var timeRegistrationService = new TimeRegistrationService(_options, _userContextMock.Object,
                CreateTaskUtils(), mockRepo.Object, new DbContextScope(sqliteContext), new PayoutStorage(sqliteContext));
            var dateToTest = new DateTime(2021, 12, 13); //Monday
            var timeEntry = CreateTimeEntryForExistingTask(dateToTest, 10M, 1);
            try
            {
                timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                    {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}});
            }
            catch (Exception e)
            {
                var earnedOvertime = timeRegistrationService.GetEarnedOvertime(new OvertimeQueryFilter
                    {StartDate = dateToTest, EndDate = dateToTest});

                Assert.Empty(sqliteContext.Hours);
                Assert.Empty(earnedOvertime);
            }
        }

        [Fact]
        public void GetOvertime_Worked7AndAHalfHours_NoOvertime()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var dateToTest = new DateTime(2021, 12, 6); //Monday
            var timeEntry1 = CreateTimeEntryWithCompensationRate(dateToTest, 7.5M, 1.5M, out int taskId1);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(0, availableHours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void GetOvertime_Worked10HoursDay1And5HoursDay2NoFlexRecorded_2AndAHalfOvertime()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 10M, 1.0M, out int _); // Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 5M, 1.0M, out int _); // Tuesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(2.5M, availableHours.AvailableHoursBeforeCompensation);
            Assert.Equal(2.5M, availableHours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetOvertime_Worked5HoursBillableAnd5Hours0Point5CompRate_1Point25Overtime()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var dateToTest = new DateTime(2021, 12, 6); // Monday
            var timeEntry1 = CreateTimeEntryWithCompensationRate(dateToTest, 5M, 1.5M, out int _);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

            var timeEntry2 = CreateTimeEntryWithCompensationRate(dateToTest, 5M, 0.5M, out int _);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(1.25M, availableHours.AvailableHoursAfterCompensation);
            Assert.Equal(2.5M, availableHours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void GetOvertime_Worked9P5HoursBillableAnd1Point5CompRate_3HoursOvertime()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var dateToTest = new DateTime(2021, 12, 6); // Monday
            var timeEntry1 = CreateTimeEntryWithCompensationRate(dateToTest, 9.5M, 1.5M, out int _);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(3M, availableHours.AvailableHoursAfterCompensation);
            Assert.Equal(2M, availableHours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void
            GetOvertime_Worked5Hours0Point5CompRateAnd7P5HoursBillableAnd5Hours1CompRate_7P5HoursAfterCompensation()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var dateToTest = new DateTime(2021, 12, 6); // Monday
            var timeEntry1 = CreateTimeEntryWithCompensationRate(dateToTest, 5M, 0.5M, out int _);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

            var timeEntry2 = CreateTimeEntryWithCompensationRate(dateToTest, 5M, 1.0M, out int _);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});

            var timeEntry3 = CreateTimeEntryWithCompensationRate(dateToTest, 7.5M, 1.5M, out int _);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(7.5M, availableHours.AvailableHoursAfterCompensation);
            Assert.Equal(10M, availableHours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void GetOvertime_Worked10Hours0Point5CompRateAnd10HoursBillableAnd10Hours1CompRate_7P5Overtime()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 10M, 0.5M, out int _); // Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 10M, 1.0M, out int _); // Tuesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});

            var timeEntry3 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 8), 10M, 1.5M, out int _); // Wednesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(7.5M, availableHours.AvailableHoursAfterCompensation);
            Assert.Equal(7.5M, availableHours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void GetOvertime_OvertimeAndTimeOff_0Overtime()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 10M, 1.5M, out int _); // Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 5M, 1.5M, out int _); // Tuesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});

            var flexTimeEntry =
                CreateTimeEntryForExistingTask(new DateTime(2021, 12, 7), 2.5M, 18); // Wednesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(0M, availableHours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void GetOvertime_OvertimeAndRegisteredPayout_5OvertimeLeft()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 17.5M, 1.0M, out int _); // Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

            var payoutService = CreatePayoutService(timeRegistrationService);
            payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 07), // Tuesday
                Hours = 5
            });

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();
            
            Assert.Equal(5M, availableHours.AvailableHoursBeforeCompensation);
            Assert.Equal(5M, availableHours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetOvertime_OvertimeAndRegisteredPayoutVariousCompRates_10OvertimeLeft()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 17.5M, 1.0M, out int _); // Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 12.5M, 1.5M, out int _); // Tuesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});
            
            var timeEntry3 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 9M, 0.5M, out int _); // Tuesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId}});

            var payoutService = CreatePayoutService(timeRegistrationService);
            var registeredPayout = payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 07), // Wednesday
                Hours = 11
            });

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();
            
            Assert.Equal(15.5M, availableHours.AvailableHoursAfterCompensation);
            Assert.Equal(13M, availableHours.AvailableHoursBeforeCompensation);
            Assert.Equal(6.5M, registeredPayout.HoursAfterCompensation);
        }

        [Fact]
        public void GetOvertime_RegisterOvertimeSameDateAsStartDate_5OvertimeAfterComp()
        {
            _context.User.First().StartDate = new DateTime(2021, 12, 6);
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 12.5M, 1.0M, out int _); // Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});
            
            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(5M, availableHours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetOvertime_FlexingBeforeWorkingWithHighCompRate_WillNotSpendHighCompRateWhenFlexing()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 6), 8.5M, 1.0M, out int _); // Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 7), 6.5M, 1.0M, out int _); // Tuesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});

            var flexTimeEntry = CreateTimeEntryForExistingTask(new DateTime(2021, 12, 7), 1.0M, 18); // Tuesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId}});

            var timeEntry3 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 8), 8.5M, 1.5M, out int _); // Wednesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(1.5M, availableHours.AvailableHoursAfterCompensation);
            Assert.Equal(1.0M, availableHours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void GetOvertime_Worked2HoursOnKristiHimmelfart_3HoursInOvertimeAfterComp()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2020, 05, 21), 2M, 1.5M, out int _); // Kr.Himmelfart
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(3M, availableHours.AvailableHoursAfterCompensation);
            Assert.Equal(2M, availableHours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void GetOvertime_Worked2HoursOnKristiHimmelfartAndMay17_6HoursInOvertime()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2020, 05, 21), 2M, 1.5M, out int _); // Kr.Himmelfart - Thursday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});
            
            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 05, 17), 2M, 1.5M, out int _); // 17 May - Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(6M, availableHours.AvailableHoursAfterCompensation);
            Assert.Equal(4M, availableHours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void GetOvertime_Worked10HoursOnWorkdayAnd1HourWeekend_6P5HoursOvertimeAfterComp()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 17), 10.5M, 1.5M, out int _); // Friday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});
            
            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 18), 4M, 0.5M, out int _); // Saturday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(6.5M, availableHours.AvailableHoursAfterCompensation);
            Assert.Equal(7M, availableHours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void GetOvertime_1May2020AndSecondPinseDag2021_5HoursOvertimeAfterComp()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2020, 05, 01), 2M, 1.5M, out int _); // 1st May - Friday 
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});
            
            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 05, 24), 4M, 0.5M, out int _); // 2nd Pinseday - Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(5M, availableHours.AvailableHoursAfterCompensation);
            Assert.Equal(6M, availableHours.AvailableHoursBeforeCompensation);
        }
        
        [Fact]
        public void GetOvertimeAtDate_OnlyGetsOvertimeEarnedAtInputDate()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 12.5M, 1.5M, out int _); // Monday 
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});
            
            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 16), 12.5M, 1.5M, out int _); // Thursday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});

            var availableHoursAtFirstOvertimeWorked = timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2021, 12, 14));
            var availableHoursAfterBothOvertimesWorked = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(7.5M, availableHoursAtFirstOvertimeWorked.AvailableHoursAfterCompensation);
            Assert.Equal(10M, availableHoursAfterBothOvertimesWorked.AvailableHoursBeforeCompensation);
        }
        
        [Fact]
        public void GetFlexhours_MultipleEmployees_FlexForSpecifiedEmployeeIsCalculated()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 12.5M, 1.5M, out int _); // Monday 
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});
            
            var user2 = new AlvTime.Business.Models.User
            {
                Id = 2,
                Email = "someone_else@alv.no",
                Name = "Someone Else"
            };
            _userContextMock.Setup(context => context.GetCurrentUser()).Returns(user2);
            
            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 12.5M, 1.5M, out int _); // Thursday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(5M, availableHours.AvailableHoursBeforeCompensation);
            Assert.Equal(7.5M, availableHours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void GetFlexhoursToday_EntriesBeforeStartDate_OvertimeIsGiven()
        {
            var dateToTest = _context.User.First(user => user.Id == 1).StartDate.AddDays(-1);
            
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(dateToTest, 12.5M, 1.5M, out int _);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.True(availableHours.AvailableHoursBeforeCompensation > 0);
        }

        [Fact]
        public void GetFlexhours_NotWorkedInWeekend_NoImpactOnOverTimeNorFlex()
        {
            // saturday and sunday:
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 18), 0M, 1.5M, out int _);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});
            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 19), 0M, 1.5M, out int _);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId}});

            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(0, availableHours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void RegisterFlexEntry_FlexingMoreThanAvailable_CannotFlex()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 8.5M, 1.5M, out int _);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId}});
            var flexTimeEntry =
                CreateTimeEntryForExistingTask(new DateTime(2021, 12, 14), 2M, 18);
            Assert.Throws<Exception>(() => timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId}}));
            
            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(1M, availableHours.AvailableHoursBeforeCompensation);
            Assert.Equal(1.5M, availableHours.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void RegisterFlexEntry_FlexingBeforeRecordedHours_CannotFlex()
        { 
            var timeRegistrationService = CreateTimeRegistrationService();

            var flexTimeEntry =
                CreateTimeEntryForExistingTask(new DateTime(2021, 12, 14), 1M, 18);
            Assert.Throws<Exception>(() => timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId}}));
        }

        [Fact]
        public void GetOvertime_RecordedVacationOnChristmas_NoFlexHours()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var vacationEntry =
                CreateTimeEntryForExistingTask(new DateTime(2021, 12, 24), 7.5M, 13);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = vacationEntry.Date, Value = vacationEntry.Value, TaskId = vacationEntry.TaskId}});
            
            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();

            Assert.Equal(0M, availableHours.AvailableHoursBeforeCompensation);
        }

        [Fact]
        public void GetAvailableHours_Worked4HoursOnWeekend_4HoursOvertimeBeforeComp6AfterComp()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 18), 4M, 1.5M, out _);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                {new() {Date = timeEntry.Date, Value = timeEntry.Value, TaskId = timeEntry.TaskId}});
            
            var availableHours = timeRegistrationService.GetAvailableOvertimeHoursNow();
            
            Assert.Equal(4M, availableHours.AvailableHoursBeforeCompensation);
            Assert.Equal(6M, availableHours.AvailableHoursAfterCompensation);
        }
        
              [Fact]
        public void GetOvertime_RegisteringVariousOvertimePayoutsAndFlex_CorrectOvertimeAtAllTimes()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 10.5M, 1.5M, out int taskId1); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

            var availableOvertime1 =
                timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2021, 12, 13));
            Assert.Equal(4.5M, availableOvertime1.AvailableHoursAfterCompensation);
            
            var flexTimeEntry =
                CreateTimeEntryForExistingTask(new DateTime(2021, 12, 14), 2, 18); //Tuesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = flexTimeEntry.Date, Value = flexTimeEntry.Value, TaskId = flexTimeEntry.TaskId } });
            
            var availableOvertime2 =
                timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2021, 12, 14));
            Assert.Equal(1.5M, availableOvertime2.AvailableHoursAfterCompensation);
            
            var timeEntry2 =
                CreateTimeEntryForExistingTask(new DateTime(2021, 12, 13), 11.5M, taskId1); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } });
            
            var availableOvertime3 =
                timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2021, 12, 14));
            Assert.Equal(3M, availableOvertime3.AvailableHoursAfterCompensation);
            
            var payoutService = CreatePayoutService(timeRegistrationService);
            payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 15), //Wednesday
                Hours = 1
            });
            
            var availableOvertime4 =
                timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2021, 12, 15));
            Assert.Equal(1.5M, availableOvertime4.AvailableHoursAfterCompensation);

            var timeEntry3 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 16), 10.5M, 0.5M, out _); //Thursday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId } });

            var availableOvertime5 =
                timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2021, 12, 16));
            Assert.Equal(3M, availableOvertime5.AvailableHoursAfterCompensation);
            
            payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 17), //Friday
                Hours = 2
            });
            
            var availableOvertime6 =
                timeRegistrationService.GetAvailableOvertimeHoursNow();
            Assert.Equal(2M, availableOvertime6.AvailableHoursAfterCompensation);
        }

        private TimeRegistrationService CreateTimeRegistrationService()
        {
            return new TimeRegistrationService(_options, _userContextMock.Object, CreateTaskUtils(),
                new TimeRegistrationStorage(_context), new DbContextScope(_context), new PayoutStorage(_context));
        }
        
        private PayoutService CreatePayoutService(TimeRegistrationService timeRegistrationService)
        {
            return new PayoutService(new PayoutStorage(_context), _userContextMock.Object,
                timeRegistrationService);
        }

        private TaskUtils CreateTaskUtils()
        {
            return new TaskUtils(new TaskStorage(_context), _options);
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

        private Hours CreateTimeEntryWithCompensationRate(DateTime date, decimal value, decimal compensationRate,
            out int taskId)
        {
            taskId = new Random().Next(1000, 10000000);
            var task = new Task {Id = taskId, Project = 1,};
            _context.Task.Add(task);
            _context.CompensationRate.Add(new CompensationRate
                {TaskId = taskId, Value = compensationRate, FromDate = new DateTime(2019, 01, 01)});
            _context.SaveChanges();

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