using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Linq;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Overtime;
using AlvTime.Business.Utils;
using FluentValidation;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static Tests.UnitTests.Flexihours.GetOvertimeTests;

namespace Tests.UnitTests.Flexihours
{
    public class RegisterPaidOvertimeTests
    {
        private readonly AlvTime_dbContext context;
        private readonly IOptionsMonitor<TimeEntryOptions> options;

        public RegisterPaidOvertimeTests()
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

        [Fact]
        public void GetRegisteredPayouts_Registered10Hours_10HoursRegistered()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 17.5M, out int taskid));
            context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 10
            }, 1);

            var registeredPayouts = calculator.GetRegisteredPayouts(1);

            Assert.Equal(10, registerOvertimeResponse.HoursBeforeCompensation);
            Assert.Equal(10, registeredPayouts.TotalHoursBeforeCompRate);
        }

        [Fact]
        public void GetRegisteredPayouts_Registered3Times_ListWith5Items()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 17.5M, out int taskid));
            context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 3
            }, 1);
            calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 3
            }, 1);
            calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 4
            }, 1);

            var registeredPayouts = calculator.GetRegisteredPayouts(1);

            Assert.Equal(3, registeredPayouts.Entries.Count());
        }

        [Fact]
        public void RegisterPayout_CalculationCorrectForBeforeAndAfterCompRate()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M, out int taskid));
            context.CompensationRate.Add(CreateCompensationRate(taskid, 2.0M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 17.5M, out int taskid2));
            context.CompensationRate.Add(CreateCompensationRate(taskid2, 0.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var registeredPayout = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 03),
                Hours = 10
            }, 1);

            Assert.Equal(5, registeredPayout.HoursAfterCompensation);
        }

        [Fact]
        public void RegisterPayout_CalculationCorrectForBeforeAndAfterCompRate2()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 8.5M, out int taskid));
            context.CompensationRate.Add(CreateCompensationRate(taskid, 1.5M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 12.5M, out int taskid2));
            context.CompensationRate.Add(CreateCompensationRate(taskid2, 0.5M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 9M, out int taskid3));
            context.CompensationRate.Add(CreateCompensationRate(taskid3, 1.0M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 07), value: 9.5M, out int taskid4));
            context.CompensationRate.Add(CreateCompensationRate(taskid4, 1.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            var registeredPayout = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 07),
                Hours = 6
            }, 1);

            Assert.Equal(3.5M, registeredPayout.HoursAfterCompensation);
            Assert.Equal(6M, registeredPayout.HoursBeforeCompensation);
        }

        [Fact]
        public void RegisterPayout_NotEnoughOvertime_CannotRegisterPayout()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 11.5M, out int taskid));
            context.CompensationRate.Add(CreateCompensationRate(taskid, 1.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            Assert.Throws<ValidationException>(() => calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 7
            }, 1));
        }

        [Fact]
        public void RegisterPayout_RegisteringPayoutBeforeWorkingOvertime_NoPayout()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 11.5M, out int taskid));
            context.CompensationRate.Add(CreateCompensationRate(taskid, 1.5M));

            context.SaveChanges();

            FlexhourStorage calculator = CreateFlexhourStorage();

            Assert.Throws<ValidationException>(() => calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 1
            }, 1));
        }

        [Fact]
        public void RegisterPayout_WorkingOvertimeAfterPayout_OnlyConsiderOvertimeWorkedBeforePayout()
        {
            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 7.5M, out int taskid));
            context.CompensationRate.Add(CreateCompensationRate(taskid, 1.5M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 3M, out int taskid2));
            context.CompensationRate.Add(CreateCompensationRate(taskid2, 0.5M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 1.5M, out int taskid4));
            context.CompensationRate.Add(CreateCompensationRate(taskid4, 1.0M));

            context.SaveChanges();

            FlexhourStorage storage = CreateFlexhourStorage();

            var result = storage.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 07),
                Hours = 4
            }, 1);

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 08), value: 7.5M, out int taskid5));
            context.CompensationRate.Add(CreateCompensationRate(taskid5, 1.5M));

            context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 01, 08), value: 1.5M, out int taskid6));
            context.CompensationRate.Add(CreateCompensationRate(taskid6, 0.5M));

            context.SaveChanges();

            var overtimeEntriesAtPayoutDate = storage.GetAvailableHours(1, new DateTime(2020, 01, 01), new DateTime(2020, 01, 07));
            var payoutEntriesAtPayoutDate = overtimeEntriesAtPayoutDate.Entries.Where(e => e.Hours < 0).GroupBy(
                hours => hours.CompensationRate,
                hours => hours,
                (cr, hours) => new
                {
                    CompensationRate = cr,
                    Hours = hours.Sum(h => h.Hours)
                });

            var overtimeEntriesAfterPayoutDate = storage.GetAvailableHours(1, new DateTime(2020, 01, 01), new DateTime(2020, 01, 08));
            var payoutEntriesAfterPayoutDate = overtimeEntriesAfterPayoutDate.Entries.Where(e => e.Hours < 0).GroupBy(
                hours => hours.CompensationRate,
                hours => hours,
                (cr, hours) => new
                {
                    CompensationRate = cr,
                    Hours = hours.Sum(h => h.Hours)
                });

            Assert.Equal(payoutEntriesAfterPayoutDate, payoutEntriesAtPayoutDate);
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

            return new OvertimeService(new OvertimeStorage(context), mockUserContext.Object, new TaskStorage(context), options, CreateTaskUtils());
        }
        
        private TaskUtils CreateTaskUtils()
        {
            return new TaskUtils(new TaskStorage(context), options);
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
