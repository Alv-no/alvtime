using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Options;
using AlvTime.Business.Services;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.EconomyDataDBModels;
using AlvTime.Persistence.Repositories;
using AlvTime.Persistence.Repositories.AlvEconomyData;
using Xunit;

namespace Tests.UnitTests.Flexihours
{
    public class RegisterPaidOvertimePayoutTests
    {
        private readonly AlvEconomyDataContext _economyDataContext =
            new AlvEconomyDataDbContextBuilder().CreateDbContext();

        private readonly AlvTime_dbContext _context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();

        #region happycases
        [Fact]
        public void RegisterOvertimePayoutSalary_1HourOvertimeWith1SalaryAndCompRate1_SalaryRegistered()
        {
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = null
            });

            _economyDataContext.SaveChanges();

            var sut = CreateStorage();

            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry { Date = new DateTime(2020, 01, 02), Hours = 1.0M }, 
                1);

            Assert.Equal(300.0M, overtimeSalary);
        }


        [Fact]
        public void RegisterOvertimePayoutSalary_2HoursOvertimeWith2DifferentSalariesSameCompRate_SalaryRegistered()
        {
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(2020, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 400.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = null
            });

            _economyDataContext.SaveChanges();

            var sut = CreateStorage();

            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 1.0M, Date = new DateTime(2021, 01, 01), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry { Date = new DateTime(2021, 08, 02), Hours = 2.0M },
                1);

            Assert.Equal(700.0M, overtimeSalary);
        }


        [Fact]
        public void RegisterOvertimePayoutSalary_2HoursOvertimeWith2DifferentCompensationRates1Salary__SalaryRegistered()
        {
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = null
            });

            _economyDataContext.SaveChanges();

            var sut = CreateStorage();

            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry { Date = new DateTime(2020, 01, 02), Hours = 2.0M },
                1);

            Assert.Equal(450.0M, overtimeSalary);
        }

        [Fact]
        public void
            RegisterOvertimePayoutSalary_2HoursOvertimeWith2DifferentSalariesAnd2DifferentCompensationRates_SalaryRegistered()
        {
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(2020, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 400.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = null
            });

            _economyDataContext.SaveChanges();

            var sut = CreateStorage();

            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2021, 01, 01), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry { Date = new DateTime(2020, 01, 02), Hours = 2.0M },
                1);

            Assert.Equal(500.0M, overtimeSalary);
        }

        [Fact]
        public void RegisterOvertimePayoutSalary_3hoursOvertime3SalariesOvertimeFrom1Salary_SalaryRegistered()
        {

            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(2020, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = new DateTime(2021, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1001,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2021),
                ToDate = null
            });

            _economyDataContext.SaveChanges();

            var sut = CreateStorage();

            var overtimeSalary = sut.RegisterOvertimePayout(
                new List<OvertimeEntry>
                {
                    new() {CompensationRate = 1.0M, Date = new DateTime(2020, 01, 01), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2020, 01, 02), Hours = 1, TaskId = 1},
                    new() {CompensationRate = 0.5M, Date = new DateTime(2020, 01, 03), Hours = 1, TaskId = 1}
                },
                1001,
                new GenericHourEntry { Date = new DateTime(2021, 07, 02), Hours = 3.0M },
                1);

            Assert.Equal(600.0M, overtimeSalary);
        }

        [Fact]
        public void RegisterPaidOvertime_UserHasOvertimeThreeDifferentSalariesAndRequestPaidOvertimeTwiceFromFirstSalary_Ok()
        {
            var userId = 1;
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(2020, 06, 30)
            };
            var secondSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = new DateTime(2021, 06, 30)
            };
            var thirdSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 250.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2021),
                ToDate = null
            };
            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(thirdSalary);
            _economyDataContext.SaveChanges();

            //overtime day 1:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 4.5M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 1.0M));

            _context.SaveChanges();

            //overtime day 2:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 7.5M, out int taskid3));
            _context.CompensationRate.Add(CreateCompensationRate(taskid3, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 9.0M, out int taskid4));
            _context.CompensationRate.Add(CreateCompensationRate(taskid4, 1.0M));

            _context.SaveChanges();

            //overtime day 3:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(CreateCompensationRate(taskid5, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 1.5M, out int taskid6));
            _context.CompensationRate.Add(CreateCompensationRate(taskid6, 1.0M));

            _context.SaveChanges();

            //act
            var sut = CreateStorage();

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
            var overtidsutbetalingEn =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 1);
            var overtidsutbetalingTo =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 2);

            Assert.Equal(100M, overtidsutbetalingEn.TotalPayout);
            Assert.Equal(200.0M, overtidsutbetalingTo.TotalPayout);
        }


        [Fact]
        public void RegisterPaidOvertime_UserHasOvertimeThreeDifferentSalariesAndRequestPaidOvertimeFromEachSalary_Ok()
        {

            var userId = 1;
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(2020, 06, 30)
            };
            var secondSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = new DateTime(2021, 06, 30)
            };
            var thirdSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 250.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2021),
                ToDate = null
            };
            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(thirdSalary);
            _economyDataContext.SaveChanges();

            //overtime day 1:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 4.5M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 1.0M));

            _context.SaveChanges();

            //overtime day 2:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 7.5M, out int taskid3));
            _context.CompensationRate.Add(CreateCompensationRate(taskid3, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 9.0M, out int taskid4));
            _context.CompensationRate.Add(CreateCompensationRate(taskid4, 1.0M));

            _context.SaveChanges();

            //overtime day 3:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(CreateCompensationRate(taskid5, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 1.5M, out int taskid6));
            _context.CompensationRate.Add(CreateCompensationRate(taskid6, 1.0M));

            _context.SaveChanges();

            //act
            var sut = CreateStorage();

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
            var overtidsutbetalingEn =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 1);
            var overtidsutbetalingTo =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 2);
            var overtidsutbetalingTre =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 3);

            Assert.Equal(450M, overtidsutbetalingEn.TotalPayout);
            Assert.Equal(1350M, overtidsutbetalingTo.TotalPayout);
            Assert.Equal(375.0M, overtidsutbetalingTre.TotalPayout);
        }
        [Fact]
        public void RegisterPaidOvertime_UserHasOvertimeThreeDifferentSalariesAndRequestAllOvertimeForPayout_Ok()
        {

            var userId = 1;
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(2020, 06, 30)
            };
            var secondSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = new DateTime(2021, 06, 30)
            };
            var thirdSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 250.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2021),
                ToDate = null
            };
            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(thirdSalary);
            _economyDataContext.SaveChanges();

            //overtime day 1:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 4.5M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 1.0M));

            _context.SaveChanges();

            //overtime day 2:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 7.5M, out int taskid3));
            _context.CompensationRate.Add(CreateCompensationRate(taskid3, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 9.0M, out int taskid4));
            _context.CompensationRate.Add(CreateCompensationRate(taskid4, 1.0M));

            _context.SaveChanges();

            //overtime day 3:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(CreateCompensationRate(taskid5, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 1.5M, out int taskid6));
            _context.CompensationRate.Add(CreateCompensationRate(taskid6, 1.0M));

            _context.SaveChanges();

            //act
            var sut = CreateStorage();

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2021, 09, 01),
                Hours = 15M
            }, userId);

            //assert
            var overtidsutbetalingEn =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 1);
            
            Assert.Equal(2175M, overtidsutbetalingEn.TotalPayout);
        }
        [Fact]
        public void RegisterPaidOvertime_UserHasOvertimeThreeoDifferentSalariesAndRequestPaidOvertimeTwiceFromFirstAndSecondSalary_Ok()
        {
            var userId = 1;
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(2020, 06, 30)
            };
            var secondSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = new DateTime(2021, 06, 30)
            };
            var thirdSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 250.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2021),
                ToDate = null
            };
            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(thirdSalary);
            _economyDataContext.SaveChanges();

            //overtime day 1:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 4.5M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 1.0M));

            _context.SaveChanges();

            //overtime day 2:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 7.5M, out int taskid3));
            _context.CompensationRate.Add(CreateCompensationRate(taskid3, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 9.0M, out int taskid4));
            _context.CompensationRate.Add(CreateCompensationRate(taskid4, 1.0M));

            _context.SaveChanges();

            //overtime day 3:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(CreateCompensationRate(taskid5, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 1.5M, out int taskid6));
            _context.CompensationRate.Add(CreateCompensationRate(taskid6, 1.0M));

            _context.SaveChanges();
            
            //act
            var sut = CreateStorage();

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
            var overtidsutbetalingEn =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 1);
            var overtidsutbetalingTo =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 2);

            Assert.Equal(450M, overtidsutbetalingEn.TotalPayout);
            Assert.Equal(300.0M, overtidsutbetalingTo.TotalPayout);
        }

        [Fact]
        public void RegisterPaidOvertime_UserHasOvertimeThreeDifferentSalariesAndRequestPaidOvertimeTwiceOverAllSalaries_Ok()
        {
            var userId = 1;
            var firstSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(2020, 06, 30)
            };
            var secondSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = new DateTime(2021, 06, 30)
            };
            var thirdSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 250.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2021),
                ToDate = null
            };
            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(thirdSalary);
            _economyDataContext.SaveChanges();

            //overtime day 1:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 4.5M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 1.0M));

            _context.SaveChanges();

            //overtime day 2:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 7.5M, out int taskid3));
            _context.CompensationRate.Add(CreateCompensationRate(taskid3, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 08, 03), value: 9.0M, out int taskid4));
            _context.CompensationRate.Add(CreateCompensationRate(taskid4, 1.0M));

            _context.SaveChanges();

            //overtime day 3:
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(CreateCompensationRate(taskid5, 1.0M));

            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 08, 04), value: 1.5M, out int taskid6));
            _context.CompensationRate.Add(CreateCompensationRate(taskid6, 1.0M));

            _context.SaveChanges();

            //act
            var sut = CreateStorage();

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
            var overtidsutbetalingEn =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 1);
            var overtidsutbetalingTo =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 2);

            Assert.Equal(825M, overtidsutbetalingEn.TotalPayout);
            Assert.Equal(1350.0M, overtidsutbetalingTo.TotalPayout);
        }

        #endregion

        #region simplecaseDifferentComprates

        [Fact]
        public void RegisterPaidOvertime_UserHasOvertimeThreeDifferentSalariesAndDifferentCompRates_Ok()
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
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));
            //comprate 1.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 1.5M, out int taskid4));
            _context.CompensationRate.Add(CreateCompensationRate(taskid4, 1.5M));
            //comprate 1.0
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 12), value: 1.5M, out int taskid3));
            _context.CompensationRate.Add(CreateCompensationRate(taskid3, 1.0M));
            //comprate 0.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 21), value: 1.5M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 0.5M));

            _context.SaveChanges();

            //act
            var sut = CreateStorage();

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 07, 07),
                Hours = 1.0M
            }, userId);

            //assert
            var overtidsutbetalingEn =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 1);
            
            Assert.Equal(50M, overtidsutbetalingEn.TotalPayout);
        }

        [Fact]
        public void RegisterPaidOvertime_UserHasOvertimeThreeDifferentSalariesAndDifferentCompRatesTwoHours_Ok()
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
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.0M));
            //comprate 1.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 1.5M, out int taskid4));
            _context.CompensationRate.Add(CreateCompensationRate(taskid4, 1.5M));
            //comprate 1.0
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 12), value: 1.5M, out int taskid3));
            _context.CompensationRate.Add(CreateCompensationRate(taskid3, 1.0M));
            //comprate 0.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 21), value: 1.5M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 0.5M));

            _context.SaveChanges();

            //act
            var sut = CreateStorage();

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 07, 07),
                Hours = 2.0M
            }, userId);

            //assert
            var overtidsutbetalingEn =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 1);
            Assert.Equal(125M, overtidsutbetalingEn.TotalPayout);
        }
        #endregion

        #region differentCompratesWithAvspasering

        [Fact]
        public void RegisterPaidOvertime_UserHasOvertime1SalaryAndDifferentCompRatesWithAvspasering_Ok()
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
            
            var FlexTask = 18;

            //regularworking time
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.5M));
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 08), value: 6.5M, out int taskid5));
            _context.CompensationRate.Add(CreateCompensationRate(taskid5, 1.5M));
            
            //overtime
            //comprate 1.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 1.5M, out int taskid4));
            _context.CompensationRate.Add(CreateCompensationRate(taskid4, 1.5M));
            //comprate 1.0
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 1.5M, out int taskid3));
            _context.CompensationRate.Add(CreateCompensationRate(taskid3, 1.0M));
            //comprate 0.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 1.5M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 0.5M));

            //avspassering
            _context.Hours.Add(new Hours
            {
                User = userId,
                Date = new DateTime(2020,06,08),Value = 1.0M ,
                Task = new Task{Id = 18}
            });

            _context.CompensationRate.Add(CreateCompensationRate(FlexTask, 1.0M));

            _context.SaveChanges();
            
            //act
            var sut = CreateStorage();
            
            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 07, 07),
                Hours = 3M
            }, userId);

            //assert
            var overtidsutbetalingEn =
                _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 1);

            Assert.Equal(225M, overtidsutbetalingEn.TotalPayout);
        }
        
        [Fact]
        public void RegisterPaidOvertime_UserHasOvertime2SalariesAndDifferentCompRatesWithTimeoff_Ok()
        {
            var userId = 1;
            var avspasseringstask = new Task {Id = 18};
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
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = null
            };

            var avspasseringTimeEntryFirst = new Hours
            {
                User = userId,
                Date = new DateTime(2020, 06, 29),
                Value = 1.5M,
                Task = avspasseringstask
            };

            var avspasseringTimeEntrySecond = new Hours
            {
                User = userId,
                Date = new DateTime(2020, 07, 28),
                Value = 1.5M,
                Task = avspasseringstask
            };

            _economyDataContext.EmployeeHourlySalaries.Add(firstSalary);
            _economyDataContext.EmployeeHourlySalaries.Add(secondSalary);
            _economyDataContext.SaveChanges();
            
            //overtime
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.5M));
            //comprate 1.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 2.5M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 1.5M));
            //comprate 1.0
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 0.5M, out int taskid3));
            _context.CompensationRate.Add(CreateCompensationRate(taskid3, 1.0M));
            //comprate 0.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 3.5M, out int taskid4));
            _context.CompensationRate.Add(CreateCompensationRate(taskid4, 0.5M));
            _context.SaveChanges();

            //overtime
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(CreateCompensationRate(taskid5, 1.5M));
            //comprate 1.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 2.5M, out int taskid6));
            _context.CompensationRate.Add(CreateCompensationRate(taskid6, 1.5M));
            //comprate 1.0
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 0.5M, out int taskid7));
            _context.CompensationRate.Add(CreateCompensationRate(taskid7, 1.0M));
            //comprate 0.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 3.5M, out int taskid8));
            _context.CompensationRate.Add(CreateCompensationRate(taskid8, 0.5M));
            
            //avspassering etc.
            _context.Hours.Add(avspasseringTimeEntryFirst);
            _context.Hours.Add(avspasseringTimeEntrySecond);
            _context.CompensationRate.Add(CreateCompensationRate(avspasseringstask.Id, 1.0M));
            _context.SaveChanges();

            //act
            var sut = CreateStorage();

            var paidOvertimeFirst = sut.RegisterPaidOvertime(new GenericHourEntry
            {
                Date = new DateTime(2020, 07, 30),
                Hours = 10M
            }, userId);

            //assert
            var overtidsutbetalingEn = _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 1);
            Assert.Equal(1012.5M, overtidsutbetalingEn.TotalPayout);
        }

        [Fact]
        public void RegisterPaidOvertime_UserHasOvertime3SalariesAndDifferentCompRatesOvertimePayoutAndAvspasering_Ok()
        {
            var userId = 1;
            var avspasseringstask = new Task { Id = 18 };
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
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = new DateTime(day: 30, month: 06, year: 2021)
            };

            var thirdSalary = new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 170.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2021),
                ToDate = null
            };

            var avspasseringTimeEntryFirst = new Hours
            {
                User = userId,
                Date = new DateTime(2020, 06, 29),
                Value = 3.0M,
                Task = avspasseringstask
            };

            var avspasseringTimeEntrySecond = new Hours
            {
                User = userId,
                Date = new DateTime(2020, 07, 28),
                Value = 4.0M,
                Task = avspasseringstask
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
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 7.5M, out int taskid));
            _context.CompensationRate.Add(CreateCompensationRate(taskid, 1.5M));
            //comprate 1.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 2.5M, out int taskid2));
            _context.CompensationRate.Add(CreateCompensationRate(taskid2, 1.5M));
            //comprate 1.0
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 0.5M, out int taskid3));
            _context.CompensationRate.Add(CreateCompensationRate(taskid3, 1.0M));
            //comprate 0.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 05), value: 3.5M, out int taskid4));
            _context.CompensationRate.Add(CreateCompensationRate(taskid4, 0.5M));
            //Paid vacation
            _context.Hours.Add(paidHoliday);
            _context.CompensationRate.Add(CreateCompensationRate(paidHolidayTask.Id, 1.0M));
            _context.SaveChanges();
            //Offtime
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 06, 29), value: 4.5M, out int taskid13));
            _context.CompensationRate.Add(CreateCompensationRate(taskid13, 1.5M));
            _context.Hours.Add(avspasseringTimeEntryFirst);
            _context.CompensationRate.Add(CreateCompensationRate(avspasseringstask.Id, 1.0M));

            //SECOND SALARY
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 7.5M, out int taskid5));
            _context.CompensationRate.Add(CreateCompensationRate(taskid5, 1.5M));
            //comprate 1.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 2.5M, out int taskid6));
            _context.CompensationRate.Add(CreateCompensationRate(taskid6, 1.5M));
            //comprate 1.0
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 0.5M, out int taskid7));
            _context.CompensationRate.Add(CreateCompensationRate(taskid7, 1.0M));
            //comprate 0.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 07, 06), value: 3.5M, out int taskid8));
            _context.CompensationRate.Add(CreateCompensationRate(taskid8, 0.5M));
            //Offtime
            _context.Hours.Add(avspasseringTimeEntrySecond);
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2020, 07, 28), value: 3.5M, out int taskid14));
            _context.CompensationRate.Add(CreateCompensationRate(taskid14, 1.0M));
            //Sickday
            _context.Hours.Add(sickdays);
            _context.CompensationRate.Add(CreateCompensationRate(sickdaysTask.Id, 1.0M));
            _context.SaveChanges();

            //THIRD SALARY
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 09, 06), value: 7.5M, out int taskid9));
            _context.CompensationRate.Add(CreateCompensationRate(taskid9, 1.5M));
            //comprate 1.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 09, 06), value: 5.5M, out int taskid10));
            _context.CompensationRate.Add(CreateCompensationRate(taskid10, 1.5M));
            //comprate 1.0
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 09, 06), value: 0.5M, out int taskid11));
            _context.CompensationRate.Add(CreateCompensationRate(taskid11, 1.0M));
            //comprate 0.5
            _context.Hours.Add(CreateTimeEntry(date: new DateTime(2021, 09, 06), value: 3.5M, out int taskid12));
            _context.CompensationRate.Add(CreateCompensationRate(taskid12, 0.5M));
            _context.SaveChanges();

            //act
            var sut = CreateStorage();

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
            var overtidsutbetalingEn = _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 1);
            var overtidsutbetalingTo = _economyDataContext.OvertimePayouts.FirstOrDefault(x => x.RegisteredPaidOvertimeId == 2);
            Assert.Equal(50M, overtidsutbetalingEn.TotalPayout);
            Assert.Equal(380M, overtidsutbetalingTo.TotalPayout);
        }


        #endregion
        private FlexhourStorage CreateStorage()
        {
            return new FlexhourStorage(new TimeEntryStorage(_context), _context, new GetOvertimeTests.TestTimeEntryOptions(
                    new TimeEntryOptions
                    {
                        FlexTask = 18,
                        ReportUser = 11,
                        StartOfOvertimeSystem = new DateTime(2020, 01, 01)
                    }),
                new SalaryService(new OvertimePayoutStorage(_economyDataContext), new EmployeeHourlySalaryStorage(_economyDataContext, _context)));
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