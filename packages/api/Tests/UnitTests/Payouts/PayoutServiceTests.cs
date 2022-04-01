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
using FluentValidation;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Tests.UnitTests.Payouts
{
    public class PayoutServiceTests
    {
        private readonly AlvTime_dbContext _context;
        private readonly IOptionsMonitor<TimeEntryOptions> _options;
        private readonly Mock<IUserContext> _userContextMock;

        public PayoutServiceTests()
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
        public void GetRegisteredPayouts_Registered10Hours_10HoursRegistered()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 17.5M, 1.0M, out int taskId1); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

            var payoutService = CreatePayoutService(timeRegistrationService);
            var registerOvertimeResponse = payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2022, 12, 13),
                Hours = 10
            });

            var registeredPayouts = payoutService.GetRegisteredPayouts();

            Assert.Equal(10, registerOvertimeResponse.HoursBeforeCompensation);
            Assert.Equal(10, registeredPayouts.TotalHoursBeforeCompRate);
            Assert.Equal(10, registeredPayouts.TotalHoursAfterCompRate);
        }

        [Fact]
        public void GetRegisteredPayouts_Registered3Times_ListWith3Items()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 17.5M, 1.0M, out int taskId1); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

            var payoutService = CreatePayoutService(timeRegistrationService);

            payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 13),
                Hours = 3
            });
            payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 14),
                Hours = 3
            });
            payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 15),
                Hours = 4
            });

            var registeredPayouts = payoutService.GetRegisteredPayouts();

            Assert.Equal(3, registeredPayouts.Entries.Count());
        }

        [Fact]
        public void RegisterPayout_CalculationCorrectForBeforeAndAfterCompRate()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 10M, 2.0M, out _); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 14), 17.5M, 0.5M, out _); //Tuesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } });

            var payoutService = CreatePayoutService(timeRegistrationService);

            var registeredPayout = payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 14),
                Hours = 10
            });

            Assert.Equal(5, registeredPayout.HoursAfterCompensation);
        }

        [Fact]
        public void RegisterPayout_CalculationCorrectForBeforeAndAfterCompRate2()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 8.5M, 1.5M, out _); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

            var timeEntry2 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 14), 12.5M, 0.5M, out _); //Tuesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } });

            var timeEntry3 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 15), 9.0M, 1.0M, out _); //Wednesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId } });

            var timeEntry4 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 16), 9.5M, 1.5M, out _); //Thursday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry4.Date, Value = timeEntry4.Value, TaskId = timeEntry4.TaskId } });

            var payoutService = CreatePayoutService(timeRegistrationService);

            var registeredPayout = payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 16),
                Hours = 6
            });

            Assert.Equal(3.5M, registeredPayout.HoursAfterCompensation);
            Assert.Equal(6M, registeredPayout.HoursBeforeCompensation);
        }

        [Fact]
        public void RegisterPayout_NotEnoughOvertime_CannotRegisterPayout()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 11.5M, 1.5M, out _); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

            var payoutService = CreatePayoutService(timeRegistrationService);

            Assert.Throws<ValidationException>(() => payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 13),
                Hours = 7
            }));
        }

        [Fact]
        public void RegisterPayout_RegisteringPayoutBeforeWorkingOvertime_NoPayout()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 11.5M, 1.5M, out _); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

            var payoutService = CreatePayoutService(timeRegistrationService);

            Assert.Throws<ValidationException>(() => payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 12),
                Hours = 1
            }));
        }

        [Fact]
        public void RegisterPayout_WorkingOvertimeAfterPayout_OnlyConsiderOvertimeWorkedBeforePayout()
        {
            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 7.5M, 1.5M, out _); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

            var timeEntry2 = CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 3M, 0.5M, out _); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry2.Date, Value = timeEntry2.Value, TaskId = timeEntry2.TaskId } });

            var timeEntry3 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 13), 1.5M, 1.0M, out _); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry3.Date, Value = timeEntry3.Value, TaskId = timeEntry3.TaskId } });

            var payoutService = CreatePayoutService(timeRegistrationService);
            payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(2021, 12, 14), //Tuesday
                Hours = 4
            });

            var timeEntry4 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 15), 7.5M, 1.5M, out _); //Wednesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry4.Date, Value = timeEntry4.Value, TaskId = timeEntry4.TaskId } });

            var timeEntry5 =
                CreateTimeEntryWithCompensationRate(new DateTime(2021, 12, 15), 2M, 0.5M, out _); //Wednesday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry5.Date, Value = timeEntry5.Value, TaskId = timeEntry5.TaskId } });

            var availableHoursAtPayoutDate =
                timeRegistrationService.GetAvailableOvertimeHoursAtDate(new DateTime(2021, 12, 14));
            var payoutEntriesAtPayoutDate = availableHoursAtPayoutDate.Entries.Where(e => e.Hours < 0).GroupBy(
                hours => hours.CompensationRate,
                hours => hours,
                (cr, hours) => new
                {
                    CompensationRate = cr,
                    Hours = hours.Sum(h => h.Hours)
                });

            var availableHoursAfterPayoutDate = timeRegistrationService.GetAvailableOvertimeHoursNow();
            var payoutEntriesAfterPayoutDate = availableHoursAfterPayoutDate.Entries.Where(e => e.Hours < 0).GroupBy(
                hours => hours.CompensationRate,
                hours => hours,
                (cr, hours) => new
                {
                    CompensationRate = cr,
                    Hours = hours.Sum(h => h.Hours)
                });

            Assert.Equal(payoutEntriesAfterPayoutDate, payoutEntriesAtPayoutDate);
            Assert.Equal(0.5M, availableHoursAtPayoutDate.AvailableHoursAfterCompensation);
            Assert.Equal(1.5M, availableHoursAfterPayoutDate.AvailableHoursAfterCompensation);
        }

        [Fact]
        public void CancelPayout_PayoutIsRegisteredInSameMonth_PayoutIsCanceled()
        {
            var currentMonth = DateTime.Now.Month;
            var currentYear = DateTime.Now.Year;

            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(currentYear, currentMonth, 02), 17.5M, 1.0M,
                    out _); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

            var payoutService = CreatePayoutService(timeRegistrationService);

            payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(currentYear, currentMonth, 02),
                Hours = 1
            });

            payoutService.CancelPayout(new DateTime(currentYear, currentMonth, 02));

            var activePayouts = payoutService.GetRegisteredPayouts();
            Assert.Empty(activePayouts.Entries);
        }

        [Fact]
        public void CancelPayout_PayoutIsRegisteredPreviousMonth_PayoutIsLocked()
        {
            var currentYear = DateTime.Now.Year;
            var previousMonth = DateTime.Now.AddMonths(-1).Month;
            var yearToTest = previousMonth == 12 ? currentYear - 1 : currentYear;

            var timeRegistrationService = CreateTimeRegistrationService();
            var timeEntry1 =
                CreateTimeEntryWithCompensationRate(new DateTime(yearToTest, previousMonth, 02), 17.5M, 1.0M,
                    out _); //Monday
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = timeEntry1.Date, Value = timeEntry1.Value, TaskId = timeEntry1.TaskId } });

            var payoutService = CreatePayoutService(timeRegistrationService);

            payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(yearToTest, previousMonth, 02),
                Hours = 5
            });

            Assert.Throws<ValidationException>(() => payoutService.CancelPayout(new DateTime(currentYear, previousMonth, 02)));
        }
        
        [Fact]
        public void RegisterPayout_HasFutureFlex_ExceptionThrown()
        {
            var currentYear = DateTime.Now.Year;
            var currentMonth = DateTime.Now.Month;
            var currentDay = DateTime.Now.Day;
            var timeRegistrationService = CreateTimeRegistrationService();
            var overtimeEntry =
                CreateTimeEntryWithCompensationRate(new DateTime(currentYear, currentMonth, currentDay), 12M, 1.5M, out int taskId1);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = overtimeEntry.Date, Value = overtimeEntry.Value, TaskId = overtimeEntry.TaskId } });

            var futureDayToRegisterFlexOn = DateTime.Now.AddDays(5).DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday ? DateTime.Now.AddDays(7).Day : DateTime.Now.AddDays(5).Day;
            var futureFlex =
                CreateTimeEntryForExistingTask(new DateTime(currentYear, currentMonth, futureDayToRegisterFlexOn), 1M, 18);
            timeRegistrationService.UpsertTimeEntry(new List<CreateTimeEntryDto>
                { new() { Date = futureFlex.Date, Value = futureFlex.Value, TaskId = futureFlex.TaskId } });
            
            var payoutService = CreatePayoutService(timeRegistrationService);
            Assert.Throws<ValidationException>(() => payoutService.RegisterPayout(new GenericHourEntry
            {
                Date = new DateTime(currentYear, currentMonth, currentDay),
                Hours = 1M
            }));
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

        private Hours CreateTimeEntryWithCompensationRate(DateTime date, decimal value, decimal compensationRate,
            out int taskId)
        {
            taskId = new Random().Next(1000, 10000000);
            var task = new Task { Id = taskId, Project = 1, };
            _context.Task.Add(task);
            _context.CompensationRate.Add(new CompensationRate
                { TaskId = taskId, Value = compensationRate, FromDate = new DateTime(2021, 01, 01) });
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
    }
}