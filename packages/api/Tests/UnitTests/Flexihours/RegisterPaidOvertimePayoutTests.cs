using System;
using System.Linq;
using AlvTime.Business.FlexiHours;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.EconomyDataDBModels;
using Xunit;

namespace Tests.UnitTests.Flexihours
{
    public class RegisterPaidOvertimePayoutTests
    {
        private readonly AlvEconomyDataContext _economyDataContext =
            new AlvEconomyDataDbContextBuilder().CreateDbContext();

        private readonly AlvTime_dbContext _context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();
        
        [Fact]
        public void RegisterPaidOvertime_UserHasOvertimeThreeDifferentSalariesAndRequestAllOvertimeForPayout_PayoutRegistered()
        {

            var userId = 1;
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(2019,07,01),
                ToDate = new DateTime(2020, 06, 30)
            };
            var secondSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(2020,07,01),
                ToDate = new DateTime(2021, 06, 30)
            };
            var thirdSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 250.0M,
                FromDateInclusive = new DateTime(2021, 07, 01),
                ToDate = null
            };
            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(thirdSalary);
            _economyDataContext.SaveChanges();

            //overtime day 1:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 4.5M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 1.0M));

            //overtime day 2:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 7.5M, out int taskid3));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid3, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 9.0M, out int taskid4));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid4, 1.0M));
            
            //overtime day 3:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid5, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 1.5M, out int taskid6));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid6, 1.0M));

            _context.SaveChanges();
            
            //act
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2021, 09, 01),
                Hours = 15M
            }, userId);

            //assert
            var registeredPayout1 = _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeFirst.Id
            );
            var expectedPayout = 1.0M * (4.5M * firstSalary.HourlySalary + 9M * secondSalary.HourlySalary + 1.5M * thirdSalary.HourlySalary);

            Assert.Equal(expectedPayout, registeredPayout1.TotalPayout);
        }

        [Fact]
        public void RegisterPaidOvertime_UserHasOvertimeThreeDifferentSalariesAndRequestPaidOvertimeTwiceFromFirstSalary_2PayoutsRegistered()
        {
            var userId = 1;
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(2019,07,01),
                ToDate = new DateTime(2020, 06, 30)
            };
            var secondSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(2020,07,01),
                ToDate = new DateTime(2021, 06, 30)
            };
            var thirdSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 250.0M,
                FromDateInclusive = new DateTime(2021,07,01),
                ToDate = null
            };
            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(thirdSalary);
            _economyDataContext.SaveChanges();

            //overtime day 1:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 4.5M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 1.0M));
            
            //overtime day 2:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 7.5M, out int taskid3));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid3, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 9.0M, out int taskid4));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid4, 1.0M));
            
            //overtime day 3:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid5, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 1.5M, out int taskid6));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid6, 1.0M));

            _context.SaveChanges();

            //act
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 07, 07),
                Hours = 1.0M
            }, userId);

            var paidOvertimeSecond = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2021, 08, 31),
                Hours = 2.0M
            }, userId);
            
            //assert
            var registeredPayout1 =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeFirst.Id);
            var registeredPayout2 =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeSecond.Id);

            Assert.Equal(100M*1.0M, registeredPayout1.TotalPayout);
            Assert.Equal(2M*100.0M, registeredPayout2.TotalPayout);
        }


        [Fact]
        public void RegisterPaidOvertime_UserHasOvertimeThreeDifferentSalariesAndRequestFullPaidOvertimeFromEachSalary_3PayoutsRegistered()
        {

            var userId = 1;
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(2019,07,01),
                ToDate = new DateTime(2020, 06, 30)
            };
            var secondSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(2020, 07, 01),
                ToDate = new DateTime(2021, 06, 30)
            };
            var thirdSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 250.0M,
                FromDateInclusive = new DateTime(2021, 07, 01),
                ToDate = null
            };
            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(thirdSalary);
            _economyDataContext.SaveChanges();

            //overtime day 1:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 4.5M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 1.0M));
            
            //overtime day 2:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 7.5M, out int taskid3));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid3, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 9.0M, out int taskid4));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid4, 1.0M));
            
            //overtime day 3:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid5, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 1.5M, out int taskid6));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid6, 1.0M));

            _context.SaveChanges();

            //act
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 07, 07),
                Hours = 4.5M
            }, userId);

            var paidOvertimeSecond = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2021, 08, 31),
                Hours = 9.0M
            }, userId);
            var paidOvertimeThird = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2021, 08, 31),
                Hours = 1.5M
            }, userId);

            //assert
            var registeredPayout1 =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeFirst.Id);
            var registeredPayout2 =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeSecond.Id);
            var registeredPayout3 =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeThird.Id);

            Assert.Equal(4.5M*firstSalary.HourlySalary*1M, registeredPayout1.TotalPayout);
            Assert.Equal(9M*secondSalary.HourlySalary*1M, registeredPayout2.TotalPayout);
            Assert.Equal(1.5M*thirdSalary.HourlySalary*1M, registeredPayout3.TotalPayout);
        }
        
        [Fact]
        public void RegisterPaidOvertime_UserHasOvertimeThreeDifferentSalariesAndRequestFullPayoutFromFirstSalaryAndPartialPayoutFromSecondSalary_2PayoutsRegistered()
        {
            var userId = 1;
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(2019, 07, 01),
                ToDate = new DateTime(2020, 06, 30)
            };
            var secondSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(2020, 07, 01),
                ToDate = new DateTime(2021, 06, 30)
            };
            var thirdSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 250.0M,
                FromDateInclusive = new DateTime(2021, 07, 01),
                ToDate = null
            };
            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(thirdSalary);
            _economyDataContext.SaveChanges();

            //overtime day 1:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 4.5M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 1.0M));
            
            //overtime day 2:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 7.5M, out int taskid3));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid3, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 9.0M, out int taskid4));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid4, 1.0M));
            
            //overtime day 3:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid5, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 1.5M, out int taskid6));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid6, 1.0M));

            _context.SaveChanges();

            //act
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 07, 07),
                Hours = 4.5M
            }, userId);

            var paidOvertimeSecond = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2021, 08, 31),
                Hours = 2.0M
            }, userId);

            //assert
            var registeredPayout1 =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeFirst.Id);
            var registeredPayout2 =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeSecond.Id);

            Assert.Equal(4.5M*firstSalary.HourlySalary*1M, registeredPayout1.TotalPayout);
            Assert.Equal(2M*secondSalary.HourlySalary*1M, registeredPayout2.TotalPayout);
        }

        [Fact]
        public void RegisterPaidOvertime_UserHasOvertimeThreeDifferentSalariesAndRequestPaidOvertimeTwiceOverAllSalaries_2PayoutsRegistered()
        {
            var userId = 1;
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(2019, 07, 01),
                ToDate = new DateTime(2020, 06, 30)
            };
            var secondSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(2020, 07, 01),
                ToDate = new DateTime(2021, 06, 30)
            };
            var thirdSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 250.0M,
                FromDateInclusive = new DateTime(2021, 07, 01),
                ToDate = null
            };
            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(thirdSalary);
            _economyDataContext.SaveChanges();

            //overtime day 1:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 4.5M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 1.0M));

            //overtime day 2:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 7.5M, out int taskid3));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid3, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 9.0M, out int taskid4));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid4, 1.0M));
            
            //overtime day 3:
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid5, 1.0M));

            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 1.5M, out int taskid6));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid6, 1.0M));

            _context.SaveChanges();

            //act
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2021, 07, 07),
                Hours = 7.0M
            }, userId);

            var paidOvertimeSecond = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2021, 08, 31),
                Hours = 8.0M
            }, userId);

            //assert
            var registeredPayout1 = _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeFirst.Id);
            var registeredPayout2 = _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeSecond.Id);
            
            Assert.Equal(firstSalary.HourlySalary * 1.0M * 4.5M + secondSalary.HourlySalary * 1.0M * 2.5M, registeredPayout1.TotalPayout);
            Assert.Equal(secondSalary.HourlySalary*1.0M*6.5M+thirdSalary.HourlySalary*1.0M*1.5M, registeredPayout2.TotalPayout);
        }
        
        [Fact]
        public void RegisterPaidOvertime_UserHasOneSalaryAndOvertimeWithDifferentCompRates_1PayoutRegistered()
        {
            var userId = 1;
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(2019, 07, 01),
                ToDate = null
            };

            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.SaveChanges();
            
            //overtime
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.0M));
            //comprate 1.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 1.5M, out int taskid4));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid4, 1.5M));
            //comprate 1.0
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 12), value: 1.5M, out int taskid3));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid3, 1.0M));
            //comprate 0.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 21), value: 1.5M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 0.5M));

            _context.SaveChanges();

            //act
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 07, 07),
                Hours = 1.0M
            }, userId);

            //assert
            var registeredPayout = _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeFirst.Id);
            
            Assert.Equal(1.0M*firstSalary.HourlySalary*0.5M, registeredPayout.TotalPayout);
        }

        [Fact]
        public void RegisterPaidOvertime_UserHasOneSalaryOvertimeWithDifferentCompRatesRequestsPayoutOverMoreThanOneComprate_1PayoutRegisteres()
        {
            var userId = 1;
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = null
            };

            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.SaveChanges();

            //overtime
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.0M));
            //comprate 1.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 1.5M, out int taskid4));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid4, 1.5M));
            //comprate 1.0
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 12), value: 1.5M, out int taskid3));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid3, 1.0M));
            //comprate 0.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 21), value: 1.5M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 0.5M));

            _context.SaveChanges();

            //act
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 07, 07),
                Hours = 2.0M
            }, userId);

            //assert
            var registeredPayout = _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeFirst.Id);

            Assert.Equal(firstSalary.HourlySalary*(0.5M*1.5M+1.0M*0.5M), registeredPayout.TotalPayout);
        }
        
        [Fact]
        public void RegisterPaidOvertime_UserHas1SalaryAndOvertimeWithDifferentCompRatesAndFlexiHoursRequestsPayout_PayoutRegistered()
        {
            //Arrange
            var userId = 1;
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(2019, 07, 01),
                ToDate = null
            };

            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.SaveChanges();
            
            var FlexTask = 18;
            
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.5M));
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 08), value: 6.5M, out int taskid5));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid5, 1.5M));
            
            //comprate 1.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 1.5M, out int taskid4));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid4, 1.5M));
            //comprate 1.0
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 1.5M, out int taskid3));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid3, 1.0M));
            //comprate 0.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 1.5M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 0.5M));

            //flex hours
            _context.Hours.Add(new Hours
            {
                User = userId,
                Date = new DateTime(2020,06,08),Value = 1.0M ,
                Task = new Task{Id = 18}
            });

            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(FlexTask, 1.0M));

            _context.SaveChanges();

            //Act
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 07, 07),
                Hours = 3.5M
            }, userId);

            //Assert
            var registeredPayout = _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeFirst.Id);

            Assert.Equal(firstSalary.HourlySalary*(1.5M*0.5M+1.5M*1.0M+0.5M*1.5M), registeredPayout.TotalPayout);
        }
        
        [Fact]
        public void RegisterPaidOvertime_UserHas2SalariesAndDifferentCompRatesWithTimeoff_PayoutRegistered()
        {
            //Arrange:
            var userId = 1;
            var flexTask = new Task {Id = 18};
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(2019,07,01),
                ToDate = new DateTime(2020,06,30)
            };

            var secondSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(2020, 07, 01),
                ToDate = null
            };

            var flexTimeEntryFirst = new Hours
            {
                User = userId,
                Date = new DateTime(2020, 06, 29),
                Value = 1.5M,
                Task = flexTask
            };

            var flexTimeEntrySecond = new Hours
            {
                User = userId,
                Date = new DateTime(2020, 07, 28),
                Value = 1.5M,
                Task = flexTask
            };

            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);
            _economyDataContext.SaveChanges();
            
            //overtime
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.5M));
            //comprate 1.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 2.5M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 1.5M));
            //comprate 1.0
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 0.5M, out int taskid3));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid3, 1.0M));
            //comprate 0.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 3.5M, out int taskid4));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid4, 0.5M));
            _context.SaveChanges();

            //overtime
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid5, 1.5M));
            //comprate 1.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 2.5M, out int taskid6));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid6, 1.5M));
            //comprate 1.0
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 0.5M, out int taskid7));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid7, 1.0M));
            //comprate 0.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 3.5M, out int taskid8));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid8, 0.5M));
            
            //time off
            _context.Hours.Add(flexTimeEntryFirst);
            _context.Hours.Add(flexTimeEntrySecond);
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(flexTask.Id, 1.0M));
            _context.SaveChanges();

            //act
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 07, 30),
                Hours = 10M
            }, userId);

            //assert
            var registeredPayout = _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == paidOvertimeFirst.Id);

            Assert.Equal(firstSalary.HourlySalary*(3.5M*0.5M+0.5M*1M)+secondSalary.HourlySalary*(3.5M*0.5M+0.5M*1M+1.5M*2M), registeredPayout.TotalPayout);
        }

        [Fact]
        public void RegisterPaidOvertime_UserHasOvertime3SalariesAndDifferentCompRatesOvertimePayoutAndTimeOffAndSickDays_2PayoutsRegistered()
        {
            var userId = 1;
            var flexTask = new Task { Id = 18 };
            var paidHolidayTask = new Task { Id = 13 };
            var sickdaysTask = new Task { Id = 14 };

            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(2019, 07, 01),
                ToDate = new DateTime(2020, 06, 30)
            };

            var secondSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(2020, 07, 01),
                ToDate = new DateTime(2021,06,30)
            };

            var thirdSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 170.0M,
                FromDateInclusive = new DateTime(2021, 07, 01),
                ToDate = null
            };

            var flexTimeEntryFirst = new Hours
            {
                User = userId,
                Date = new DateTime(2020, 06, 29),
                Value = 3.0M,
                Task = flexTask
            };

            var flexTimeEntrySecond = new Hours
            {
                User = userId,
                Date = new DateTime(2020, 07, 28),
                Value = 4.0M,
                Task = flexTask
            };

            var paidHoliday = new Hours
            {
                User = userId,
                Date = new DateTime(2020, 06, 26),
                Value = 7.5M,
                Task = paidHolidayTask
            };

            var sickdays = new Hours
            {
                User = userId,
                Date = new DateTime(2021, 05, 19),
                Value = 7.5M,
                Task = sickdaysTask
            };

            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(thirdSalary);
            _economyDataContext.SaveChanges();

            //FIRST SALARY
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.5M));
            //comprate 1.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 2.5M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 1.5M));
            //comprate 1.0
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 0.5M, out int taskid3));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid3, 1.0M));
            //comprate 0.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 3.5M, out int taskid4));//2.5
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid4, 0.5M));
            //Paid vacation
            _context.Hours.Add(paidHoliday);
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(paidHolidayTask.Id, 1.0M));
            //Offtime
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 29), value: 4.5M, out int taskid13));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid13, 1.5M));
            _context.Hours.Add(flexTimeEntryFirst);
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(flexTask.Id, 1.0M));

            //SECOND SALARY
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid5, 1.5M));
            //comprate 1.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 2.5M, out int taskid6));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid6, 1.5M));
            //comprate 1.0
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 0.5M, out int taskid7));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid7, 1.0M));
            //comprate 0.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 3.5M, out int taskid8));//3,5
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid8, 0.5M));
            //Offtime
            _context.Hours.Add(flexTimeEntrySecond);
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 07, 28), value: 3.5M, out int taskid14));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid14, 1.0M));
            //Sickday
            _context.Hours.Add(sickdays);
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(sickdaysTask.Id, 1.0M));
            
            //THIRD SALARY
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 09, 06), value: 7.5M, out int taskid9));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid9, 1.5M));
            //comprate 1.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 09, 06), value: 5.5M, out int taskid10));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid10, 1.5M));
            //comprate 1.0
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 09, 06), value: 0.5M, out int taskid11));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid11, 1.0M));
            //comprate 0.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2021, 09, 06), value: 3.5M, out int taskid12));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid12, 0.5M));
            _context.SaveChanges();

            //act
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 07, 30),
                Hours = 1M
            }, userId);

            var paidOvertimeSecond = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2021, 09, 07),
                Hours = 5.5M
            }, userId);
            
            //assert
            var registeredPayout1 = _economyDataContext.OvertimePayouts.FirstOrDefault(overtimePayout => overtimePayout.RegisteredPaidOvertimeId == paidOvertimeFirst.Id);
            var registeredPayout2 = _economyDataContext.OvertimePayouts.FirstOrDefault(overtimePayout => overtimePayout.RegisteredPaidOvertimeId == paidOvertimeSecond.Id);
            
            Assert.Equal(1.0M*100M*0.5M, registeredPayout1.TotalPayout);
            Assert.Equal(firstSalary.HourlySalary*1.5M*0.5M+secondSalary.HourlySalary*3.5M*0.5M+thirdSalary.HourlySalary*0.5M*0.5M, registeredPayout2.TotalPayout);
        }

        [Fact]
        public void RegisterPaidOvertime_UserHas1SalaryOvertimePayoutAndTimeOffOverDifferentComprates_2PayoutRegistered()
        {
            var userId = 1;
            var flexTask = new Task { Id = 18 };
            var salary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 170.0M,
                FromDateInclusive = new DateTime(2019, 07, 01),
                ToDate = null
            };
            var flexTimeEntry = new Hours
            {
                User = userId,
                Date = new DateTime(2020, 06, 29),
                Value = 3.0M,
                Task = flexTask
            };

            _economyDataContext.EmployeeHourlySalaries.Add(salary);
            _economyDataContext.SaveChanges();
            
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid, 1.5M));
            //comprate 1.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 2M, out int taskid2));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid2, 1.5M));
            //comprate 1.0
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 10M, out int taskid3));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid3, 1.0M));
            //comprate 0.5
            _context.Hours.Add(FlexiHoursTestUtils.CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 5M, out int taskid4));
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(taskid4, 0.5M));
            _context.SaveChanges();
            //Offtime
            _context.Hours.Add(flexTimeEntry);
            _context.CompensationRate.Add(FlexiHoursTestUtils.CreateCompensationRate(flexTask.Id, 1.0M));

            //act
            var sut = FlexiHoursTestUtils.CreateStorage(_context, _economyDataContext);

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 09, 15),
                Hours = 6M
            }, userId);

            var paidOvertimeSecond = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2021, 09, 17),
                Hours = 7M
            }, userId);
            
            //assert
            var expectedPayout1 = salary.HourlySalary * (0.5M * 5 + 1.0M * 1M);
            var expectedPayout2 = salary.HourlySalary * 1.0M * 7M;

            var registeredPayout1 = _economyDataContext.OvertimePayouts.FirstOrDefault(overtimePayout => overtimePayout.RegisteredPaidOvertimeId == paidOvertimeFirst.Id);
            var registeredPayout2 = _economyDataContext.OvertimePayouts.FirstOrDefault(overtimePayout => overtimePayout.RegisteredPaidOvertimeId == paidOvertimeSecond.Id);

            Assert.Equal(expectedPayout1, registeredPayout1.TotalPayout);
            Assert.Equal(expectedPayout2, registeredPayout2.TotalPayout);
        }
        
    }
}