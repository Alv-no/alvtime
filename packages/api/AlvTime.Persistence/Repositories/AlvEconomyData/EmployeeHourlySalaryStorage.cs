using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.EconomyData;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.EconomyDataDBModels;
using FluentValidation;

namespace AlvTime.Persistence.Repositories.AlvEconomyData
{
    public class EmployeeHourlySalaryStorage : IEmployeeHourlySalaryStorage
    {
        private readonly AlvTime_dbContext _context;
        private readonly AlvEconomyDataContext _economyContext;

        public EmployeeHourlySalaryStorage(AlvEconomyDataContext economyContext, AlvTime_dbContext context)
        {
            _economyContext = economyContext;
            _context = context;
        }


        public decimal GetHouerlySalary(int userId, DateTime date)
        {
            var employeeWithSalaryData = _economyContext
                    .EmployeeHourlySalaries.Where(hs => hs.UserId == userId)
                ;
            if (!employeeWithSalaryData.Any())
                throw new ValidationException("Could not find a registered salary for this user");

            var salary = employeeWithSalaryData.FirstOrDefault(x =>
                x.FromDateInclusive <= date && (x.ToDate.HasValue && x.ToDate > date || x.ToDate == null));

            return salary.HourlySalary;
        }

        public void RegisterHourlySalary(EmployeeSalaryDto employeeSalaryData)
        {
            var employee = _context.User.FirstOrDefault(e => e.Id == employeeSalaryData.usiderId);

            if (employee == null)
                throw new ValidationException($"Could not find a an employee with id {employeeSalaryData.usiderId}");

            var employeeHasRegisteredSalary =
                _economyContext.EmployeeHourlySalaries.Any(x => x.UserId == employeeSalaryData.usiderId);

            if (employeeHasRegisteredSalary)
            {
                var currentSalary = _economyContext
                    .EmployeeHourlySalaries.FirstOrDefault(x => x.UserId == employeeSalaryData.usiderId &&
                                                                x.FromDateInclusive <= DateTime.Now &&
                                                                (x.ToDate.HasValue && x.ToDate > DateTime.Now ||
                                                                 x.ToDate == null));
                currentSalary.ToDate = employeeSalaryData.FromDate.AddDays(-1);
            }

            _economyContext.EmployeeHourlySalaries.Add(new EmployeeHourlySalary
            {
                UserId = employeeSalaryData.usiderId,
                FromDateInclusive = employeeSalaryData.FromDate,
                HourlySalary = employeeSalaryData.HourlySalary,
                ToDate = employeeSalaryData.ToDate
            });


            _economyContext.SaveChanges();
        }

        public List<EmployeeSalaryDto> GetEmployeeSalaryData(int userId)
        {
            var employeeWithSalaryData = _economyContext
                    .EmployeeHourlySalaries.Where(hs => hs.UserId == userId)
                ;
            if (!employeeWithSalaryData.Any())
                throw new ValidationException("Could not find a registered salary for this user");

            var returnSalary = new List<EmployeeSalaryDto>();
            foreach (var hourlySalary in employeeWithSalaryData) returnSalary.Add(ToEmployeeSalaryDto(hourlySalary));

            return returnSalary;
        }

        private EmployeeSalaryDto ToEmployeeSalaryDto(EmployeeHourlySalary employeeHourlySalary)
        {
            return new()
            {
                usiderId = employeeHourlySalary.UserId, FromDate = employeeHourlySalary.FromDateInclusive,
                ToDate = employeeHourlySalary.ToDate, HourlySalary = employeeHourlySalary.HourlySalary
            };
        }
    }
}