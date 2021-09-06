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
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 100.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2019),
                ToDate = new DateTime(2020, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 150.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = new DateTime(2021, 06, 30)
            });
            _economyDataContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = userId,
                HourlySalary = 250.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2021),
                ToDate = null
            });

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
                Hours = 1.0M
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
            Assert.Equal(250.0M, overtidsutbetalingTre.TotalPayout);
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
        
        private FlexhourStorage CreateStorage()
        {
            return new FlexhourStorage(new TimeEntryStorage(_context), _context, new GetOvertimeTests.TestTimeEntryOptions(
                    new TimeEntryOptions
                    {
                        FlexTask = 18,
                        ReportUser = 11,
                        StartOfOvertimeSystem = new DateTime(2021, 09, 01)
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