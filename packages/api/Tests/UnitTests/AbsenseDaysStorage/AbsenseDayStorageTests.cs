using AlvTime.Business.Options;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;
using AlvTime.Business.AbsenseDays;
using AlvTime.Persistence.Repositories;
using AlvTime.Business.TimeEntries;
using System;
using AlvTime.Business;

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
                PaidHolidayTask = 13,
                UnpaidHolidayTask = 12
            };
            options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);

        }

        [Fact]
        public void CheckRemainingHolidays_NoWithDrawls()
        {
            var absenseService = new AbsenseDaysService(new TimeEntryStorage(context), options);
            var days = absenseService.GetAbsenseDays(1, 2020);

            Assert.Equal(25, days.VacationDays);
            Assert.Equal(0, days.UsedAbsenseDays);
            Assert.Equal(0, days.UsedAlvDays);
            Assert.Equal(6, days.AlvDaysInAYear);
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

            Assert.Equal(3, days.UsedAbsenseDays);

            
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

            Assert.Equal(3, days.UsedAbsenseDays);
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

            // We withdraw three whole days
            Assert.Equal(6, days.UsedAbsenseDays);
        }

        [Fact]
        public void TestCalculationOfAlvDays() {
            var redDays = new RedDays(2021);
            var timeEntryStorage = new TimeEntryStorage(context);
            var absenseService = new AbsenseDaysService(timeEntryStorage, options);
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto {
                Date = redDays.GetTuesdayInEaster(2021),
                Value = 5,
                TaskId = options.CurrentValue.PaidHolidayTask
            }, 1);

            var days = absenseService.GetAbsenseDays(1, 2021, null);

            Assert.Equal(7, days.AlvDaysInAYear);
            // Important that ordinary vacaition days are not withdrawn
            Assert.Equal(0, days.UsedVacationDays);
            Assert.Equal(1, days.UsedAlvDays);
        }

    }
}

