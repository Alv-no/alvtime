using System;
using AlvTime.Persistence.EconomyDataDBModels;
using Microsoft.EntityFrameworkCore;

namespace Tests.UnitTests
{
    public class AlvEconomyDataDbContextBuilder
    {
        private readonly AlvEconomyDataContext _context;

        public AlvEconomyDataDbContextBuilder()
        {
            var options = new DbContextOptionsBuilder<AlvEconomyDataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _context = new AlvEconomyDataContext(options);
        }

        public AlvEconomyDataContext CreateDbContext()
        {
            return _context;
        }

        public AlvEconomyDataDbContextBuilder WithEmployeeSalary()
        {
            _context.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1,
                HourlySalary = 400.0M,
                FromDateInclusive = new DateTime(day: 01, month: 01, year: 2020),
                ToDate = new DateTime(day: 30, month: 06, year: 2020)
            });

            _context.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 1,
                HourlySalary = 500.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = null
            });

            _context.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = 2,
                HourlySalary = 300.0M,
                FromDateInclusive = new DateTime(day: 01, month: 07, year: 2020),
                ToDate = null
            });

            _context.SaveChanges();
            return this;
        }
    }
}