using AlvTime.Business.Options;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;
using AlvTime.Business.AbsenseDays;
using AlvTime.Persistence.Repositories;
using AlvTime.Business.TimeEntries;
using System;

namespace Tests.UnitTests.AbsenseDaysStorage
{
    public class AbsenseDayStorageTests
    {
        private readonly AlvTime_dbContext context;
        private readonly IOptionsMonitor<TimeEntryOptions> options;
        public AbsenseDayStorageTests()
        {
            context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .WithTasks()
                .WithTimeEntries()
                .WithHourRates()
                .CreateDbContext();

            var entryOptions = new TimeEntryOptions 
            {
                SickDaysTask = 14,
                PaidHolidayTask = 13
            };
            options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);

        }

        [Fact]
        public void CheckRemainingHolidays_NoWithDrawls()
        {
            var absenseService = new AbsenseDaysService(new TimeEntryStorage(context), options);
            var days = absenseService.GetAbsenseDays(1, 2020);

            Assert.Equal(25, days.VacationDaysLeft);
            Assert.Equal(12, days.SickLeaveDaysLeft);
        }

        [Fact]
        public void CheckRemainingHolidays_SickdaysTaken()
        {
            var timeEntryStorage = new TimeEntryStorage(context);
            var absenseService = new AbsenseDaysService(timeEntryStorage, options);

            // One day of sick leave
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto {
                Date = DateTime.Now.AddDays(-2),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

                }, 1);
            

            var days = absenseService.GetAbsenseDays(1, 2021);

            Assert.Equal(25, days.VacationDaysLeft);
            // We withdraw three whole days
            Assert.Equal(9, days.SickLeaveDaysLeft);

            
            // These two withdrawals of sick days should also count as 3 as they are concurrent
            

        }

        [Fact]
        public void TestConcurrentDays() {

            var timeEntryStorage = new TimeEntryStorage(context);
            var absenseService = new AbsenseDaysService(timeEntryStorage, options);
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto {
                Date = DateTime.Now.AddDays(-9),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

                }, 1);
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto {
                Date = DateTime.Now.AddDays(-10),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

                }, 1);
            
            var days = absenseService.GetAbsenseDays(1, 2021);

            Assert.Equal(25, days.VacationDaysLeft);
            // We withdraw three whole days
            Assert.Equal(9, days.SickLeaveDaysLeft);
        }

        [Fact]
        public void TestConcurrentDays_MoreThanThree() {

            var timeEntryStorage = new TimeEntryStorage(context);
            var absenseService = new AbsenseDaysService(timeEntryStorage, options);
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto {
                Date = DateTime.Now.AddDays(-9),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

                }, 1);
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto {
                Date = DateTime.Now.AddDays(-10),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

                }, 1);
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto {
                Date = DateTime.Now.AddDays(-11),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

                }, 1);
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto {
                Date = DateTime.Now.AddDays(-12),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

                }, 1);
            
            var days = absenseService.GetAbsenseDays(1, 2021);

            Assert.Equal(25, days.VacationDaysLeft);
            // We withdraw three whole days
            Assert.Equal(6, days.SickLeaveDaysLeft);
        }

    }
}
