using AlvTime.Business.Options;
using AlvTime.Persistence.DataBaseModels;
using Microsoft.Extensions.Options;
using Xunit;
using Moq;
using AlvTime.Business.AbsenseDays;
using AlvTime.Persistence.Repositories;
using AlvTime.Business.TimeEntries;
using System;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Overtime;
using AlvTime.Business.Utils;
using Tests.UnitTests.Flexihours;

namespace Tests.UnitTests.AbsenseDaysStorage
{
    public class AbsenseDayStorageTests
    {
        private readonly AlvTime_dbContext _context;
        private readonly IOptionsMonitor<TimeEntryOptions> _options;

        public AbsenseDayStorageTests()
        {
            _context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .WithTasks()
                .WithLeaveTasks()
                .WithTimeEntries()
                .WithHourRates()
                .CreateDbContext();

            var entryOptions = new TimeEntryOptions
            {
                SickDaysTask = 14,
                PaidHolidayTask = 13,
                UnpaidHolidayTask = 12
            };
            _options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);
        }

        [Fact]
        public void CheckRemainingHolidays_NoWithDrawls()
        {
            var timeRegistrationStorage = CreateTimeRegistrationStorage();
            var absenseService = new AbsenseDaysService(timeRegistrationStorage, _options);
            var days = absenseService.GetAbsenseDays(1, 2020, null);

            Assert.Equal(0, days.UsedAbsenseDays);
        }

        [Fact]
        public void CheckRemainingHolidays_SickdaysTaken()
        {
            var timeRegistrationStorage = CreateTimeRegistrationStorage();
            var absenseService = new AbsenseDaysService(timeRegistrationStorage, _options);

            // One day of sick leave
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-2),
                Value = 5,
                TaskId = _options.CurrentValue.SickDaysTask
            }, 1);

            var days = absenseService.GetAbsenseDays(1, 2021, null);

            Assert.Equal(3, days.UsedAbsenseDays);


            // These two withdrawals of sick days should also count as 3 as they are concurrent
        }

        [Fact]
        public void TestConcurrentDays()
        {
            var timeRegistrationStorage = CreateTimeRegistrationStorage();
            var absenseService = new AbsenseDaysService(timeRegistrationStorage, _options);
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-9),
                Value = 5,
                TaskId = _options.CurrentValue.SickDaysTask
            }, 1);
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-10),
                Value = 5,
                TaskId = _options.CurrentValue.SickDaysTask
            }, 1);

            var days = absenseService.GetAbsenseDays(1, 2021, null);

            Assert.Equal(3, days.UsedAbsenseDays);
        }

        [Fact]
        public void TestConcurrentDays_MoreThanThree()
        {
            var timeRegistrationStorage = CreateTimeRegistrationStorage();
            var absenseService = new AbsenseDaysService(timeRegistrationStorage, _options);
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-9),
                Value = 5,
                TaskId = _options.CurrentValue.SickDaysTask
            }, 1);
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-10),
                Value = 5,
                TaskId = _options.CurrentValue.SickDaysTask
            }, 1);
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-11),
                Value = 5,
                TaskId = _options.CurrentValue.SickDaysTask
            }, 1);
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = DateTime.Now.AddDays(-12),
                Value = 5,
                TaskId = _options.CurrentValue.SickDaysTask
            }, 1);

            var days = absenseService.GetAbsenseDays(1, 2021, null);

            // We withdraw three whole days
            Assert.Equal(6, days.UsedAbsenseDays);
        }

        [Fact]
        public void Test_Basic_HolidayCalculations()
        {
            var timeRegistrationStorage = CreateTimeRegistrationStorage();
            var absenseService = new AbsenseDaysService(timeRegistrationStorage, _options);

            // Create both planned and used holiday
            for (int i = 0; i < 10; i++)
            {
                timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
                {
                    Date = new DateTime(2020, 2, 5).AddDays(i * -1),
                    Value = 5,
                    TaskId = _options.CurrentValue.PaidHolidayTask
                }, 1);
            }

            var holidayOverview = absenseService.GetVacationDays(1, 2020, 2, 1);

            Assert.Equal(6, holidayOverview.UsedVacationDays);
            Assert.Equal(11, holidayOverview.PlannedVacationDays);
            Assert.Equal(15, holidayOverview.AvailableVacationDays);
        }

        [Fact]
        public void Test_Complicated_HolidayCalculations()
        {
            var timeRegistrationStorage = CreateTimeRegistrationStorage();
            var absenseService = new AbsenseDaysService(timeRegistrationStorage, _options);

            // Create both planned and used holiday
            for (int i = 0; i < 10; i++)
            {
                timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
                {
                    Date = new DateTime(2020, 6, 5).AddDays(i * -1),
                    Value = 5,
                    TaskId = _options.CurrentValue.PaidHolidayTask
                }, 1);
            }

            var holidayOverview = absenseService.GetVacationDays(1, 2020, 6, 1);

            Assert.Equal(9, holidayOverview.UsedVacationDays);
            Assert.Equal(8, holidayOverview.PlannedVacationDays);
            Assert.Equal(15, holidayOverview.AvailableVacationDays);
        }

        [Fact]
        public void Test_3VacationDaysOnAlvDays_3VacationDaysUsed()
        {
            var timeRegistrationStorage = CreateTimeRegistrationStorage();
            var absenseService = new AbsenseDaysService(timeRegistrationStorage, _options);

            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2021, 3, 29),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2021, 3, 30),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2021, 3, 31),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            var holidayOverview = absenseService.GetVacationDays(1, 2021, 6, 1);

            Assert.Equal(3, holidayOverview.UsedVacationDays);
            Assert.Equal(25, holidayOverview.AvailableVacationDays);
        }

        [Fact]
        public void Test_3VacationDaysOnAlvDaysAnd1NormalDay_1LessAvailableDay()
        {
            var timeRegistrationStorage = CreateTimeRegistrationStorage();
            var absenseService = new AbsenseDaysService(timeRegistrationStorage, _options);

            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2021, 3, 29),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2021, 3, 30),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2021, 3, 31),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2021, 4, 30),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            var holidayOverview = absenseService.GetVacationDays(1, 2021, 6, 1);

            Assert.Equal(4, holidayOverview.UsedVacationDays);
            Assert.Equal(24, holidayOverview.AvailableVacationDays);
        }

        [Fact]
        public void Test_Planned5VacationDaysNotAlvDays_20RemainingVacationDays()
        {
            var timeRegistrationStorage = CreateTimeRegistrationStorage();
            var absenseService = new AbsenseDaysService(timeRegistrationStorage, _options);

            for (int i = 0; i < 5; i++)
            {
                timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
                {
                    Date = new DateTime(2021, 9, 6).AddDays(i),
                    Value = 7.5M,
                    TaskId = _options.CurrentValue.PaidHolidayTask
                }, 1);
            }

            var holidayOverview = absenseService.GetVacationDays(1, 2021, 6, 1);

            Assert.Equal(3, holidayOverview.UsedVacationDays);
            Assert.Equal(10, holidayOverview.PlannedVacationDays);
            Assert.Equal(20, holidayOverview.AvailableVacationDays);
        }

        [Fact]
        public void Test_PlannedVacationDayOnAlvDay_PlannedDaysNotDoubled()
        {
            var timeRegistrationStorage = CreateTimeRegistrationStorage();
            var absenseService = new AbsenseDaysService(timeRegistrationStorage, _options);

            for (int i = 0; i < 5; i++)
            {
                timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
                {
                    Date = new DateTime(2021, 12, 27).AddDays(i),
                    Value = 7.5M,
                    TaskId = _options.CurrentValue.PaidHolidayTask
                }, 1);
            }

            var holidayOverview = absenseService.GetVacationDays(1, 2021, 6, 1);

            Assert.Equal(3, holidayOverview.UsedVacationDays);
            Assert.Equal(5, holidayOverview.PlannedVacationDays);
            Assert.Equal(25, holidayOverview.AvailableVacationDays);
        }
        
        [Fact]
        public void GetVacationDays_NoVacationRecorded_25AvailableDays()
        {
            var timeRegistrationStorage = CreateTimeRegistrationStorage();
            var absenseService = new AbsenseDaysService(timeRegistrationStorage, _options);

            var holidayOverview = absenseService.GetVacationDays(1, 2022, 6, 1);

            Assert.Equal(25, holidayOverview.AvailableVacationDays);
        }
                
        [Fact]
        public void GetVacationDays_Recorded3DaysVacation_22AvailableDays()
        {
            var timeRegistrationStorage = CreateTimeRegistrationStorage();
            var absenseService = new AbsenseDaysService(timeRegistrationStorage, _options);
            
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2022, 1, 10),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2022, 1, 11),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2022, 1, 12),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            var holidayOverview = absenseService.GetVacationDays(1, 2022, 6, 1);

            Assert.Equal(22, holidayOverview.AvailableVacationDays);
            Assert.Equal(3, holidayOverview.UsedVacationDays);
        }
        
        [Fact]
        public void GetVacationDays_Recorded3DaysVacationAnd2DaysInFuture_20AvailableDays()
        {
            var timeRegistrationStorage = CreateTimeRegistrationStorage();
            var absenseService = new AbsenseDaysService(timeRegistrationStorage, _options);
            
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2022, 1, 10),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2022, 1, 11),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2022, 1, 12),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);
            
            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2022, 11, 07),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            timeRegistrationStorage.CreateTimeEntry(new CreateTimeEntryDto
            {
                Date = new DateTime(2022, 11, 08),
                Value = 7.5M,
                TaskId = _options.CurrentValue.PaidHolidayTask
            }, 1);

            var holidayOverview = absenseService.GetVacationDays(1, 2022, 6, 1);

            Assert.Equal(20, holidayOverview.AvailableVacationDays);
            Assert.Equal(3, holidayOverview.UsedVacationDays);
            Assert.Equal(2, holidayOverview.PlannedVacationDays);
        }

        private TimeRegistrationStorage CreateTimeRegistrationStorage()
        {
            return new TimeRegistrationStorage(_context);
        }
    }
}