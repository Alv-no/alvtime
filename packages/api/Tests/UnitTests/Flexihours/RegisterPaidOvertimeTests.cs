using AlvTime.Business.FlexiHours;
using AlvTime.Persistence.DataBaseModels;
using System;
using System.Linq;
using AlvTime.Persistence.EconomyDataDBModels;
using FluentValidation;
using Xunit;

namespace Tests.UnitTests.Flexihours
{
    public class RegisterPaidOvertimeTests
    {
        private AlvTime_dbContext _context = new AlvTimeDbContextBuilder()
        .WithUsers()
        .CreateDbContext();

        private AlvEconomyDataContext _economyDataContext = new AlvEconomyDataDbContextBuilder().WithEmployeeSalary().CreateDbContext();

        [Fact]
        public void GetRegisteredPayouts_Registered10Hours_10HoursRegistered()
        {
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var registerOvertimeResponse = calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 10
            }, 1);

            var registeredPayouts = calculator.GetRegisteredPayouts(1);

            Assert.Equal(10, registerOvertimeResponse.HoursBeforeCompensation);
            Assert.Equal(10, registeredPayouts.TotalHours);
        }

        [Fact]
        public void GetRegisteredPayouts_Registered3Times_ListWith5Items()
        {
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 17.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.0M));

            _context.SaveChanges();

            FlexhourStorage calculator = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

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
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 10M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 2.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 17.5M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 0.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

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
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 8.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.5M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 12.5M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 0.5M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 9M, out int taskid3));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid3, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 07), value: 9.5M, out int taskid4));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid4, 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

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
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 02), value: 11.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            Assert.Throws<ValidationException>(() => calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 7
            }, 1));
        }

        [Fact]
        public void RegisterPayout_RegisteringPayoutBeforeWorkingOvertime_NoPayout()
        {
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 03), value: 11.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.5M));

            _context.SaveChanges();

            FlexhourStorage calculator = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            Assert.Throws<ValidationException>(() => calculator.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 02),
                Hours = 1
            }, 1));
        }

        [Fact]
        public void RegisterPayout_WorkingOvertimeAfterPayout_OnlyConsiderOvertimeWorkedBeforePayout()
        {
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.5M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 3M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 0.5M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 06), value: 1.5M, out int taskid4));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid4, 1.0M));

            _context.SaveChanges();

            FlexhourStorage storage = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var result = storage.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 01, 07),
                Hours = 4
            }, 1);

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 08), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid5, 1.5M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 01, 08), value: 1.5M, out int taskid6));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid6, 0.5M));

            _context.SaveChanges();

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
    }
}
