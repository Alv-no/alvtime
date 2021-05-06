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

            Assert.Equal(0, days.UsedAbsenseDays);
        }

        [Fact]
        public void CheckRemainingHolidays_SickdaysTaken()
        {
            var timeEntryStorage = new TimeEntryStorage(context);
            var absenseService = new AbsenseDaysService(timeEntryStorage, options);

            // One day of sick leave
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-2),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

            }, 1);


            var days = absenseService.GetAbsenseDays(1, 2021);

            Assert.Equal(3, days.UsedAbsenseDays);


            // These two withdrawals of sick days should also count as 3 as they are concurrent


        }

        [Fact]
        public void TestConcurrentDays()
        {

            var timeEntryStorage = new TimeEntryStorage(context);
            var absenseService = new AbsenseDaysService(timeEntryStorage, options);
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-9),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

            }, 1);
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-10),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

                }, 1);

            var days = absenseService.GetAbsenseDays(1, 2021);

            Assert.Equal(3, days.UsedAbsenseDays);
        }

        [Fact]
        public void TestConcurrentDays_MoreThanThree()
        {

            var timeEntryStorage = new TimeEntryStorage(context);
            var absenseService = new AbsenseDaysService(timeEntryStorage, options);
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-9),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

            }, 1);
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-10),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

            }, 1);
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-11),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

            }, 1);
            timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-12),
                Value = 5,
                TaskId = options.CurrentValue.SickDaysTask

                }, 1);

            var days = absenseService.GetAbsenseDays(1, 2021);

            // We withdraw three whole days
            Assert.Equal(6, days.UsedAbsenseDays);
        }

        [Fact]
        public void Test_Basic_HolidayCalculations()
        {
            var timeEntryStorage = new TimeEntryStorage(context);
            var absenseService = new AbsenseDaysService(timeEntryStorage, options);

            // Create both planned and used holiday
            for (int i = 0; i < 10; i++)
            {
                timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto
                {
                    Date = new DateTime(2020, 2, 5).AddDays(i * -1),
                    Value = 5,
                    TaskId = options.CurrentValue.PaidHolidayTask
                }, 1);
            }

            var holidayOverview = absenseService.GetVacationDays(1, 2020, 2, 1);

            Assert.Equal(6, holidayOverview.UsedVacationDays);
            Assert.Equal(4, holidayOverview.PlannedVacationDays);
            Assert.Equal(21, holidayOverview.AvailableVacationDays);
        }

        [Fact]
        public void Test_Complicated_HolidayCalculations()
        {
            var timeEntryStorage = new TimeEntryStorage(context);
            var absenseService = new AbsenseDaysService(timeEntryStorage, options);

            // Create both planned and used holiday
            for (int i = 0; i < 10; i++)
            {
                timeEntryStorage.CreateTimeEntry(new CreateTimeEntryDto
                {
                    Date = new DateTime(2020, 6, 5).AddDays(i * -1),
                    Value = 5,
                    TaskId = options.CurrentValue.PaidHolidayTask
                }, 1);
            }

            var holidayOverview = absenseService.GetVacationDays(1, 2020, 6, 1);

            Assert.Equal(9, holidayOverview.UsedVacationDays);
            Assert.Equal(4, holidayOverview.PlannedVacationDays);
            Assert.Equal(18, holidayOverview.AvailableVacationDays);
        }
    }
}

