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

        public EmployeeSalaryDto RegisterHourlySalary(EmployeeSalaryRequest employeeSalaryData)
        {
            var employee = _context.User.FirstOrDefault(e => e.Id == employeeSalaryData.UserId);
            
            if (employee == null)
                throw new ValidationException($"Could not find a an employee with id {employeeSalaryData.UserId}");

            var employeeHasRegisteredSalary =
                _economyContext.EmployeeHourlySalaries.Any(x => x.UserId == employeeSalaryData.UserId);

            if (employeeHasRegisteredSalary)
            {
                var currentSalary = _economyContext
                    .EmployeeHourlySalaries.FirstOrDefault(x => x.UserId == employeeSalaryData.UserId &&
                                                                x.FromDateInclusive <= DateTime.Now &&
                                                                (x.ToDate.HasValue && x.ToDate > DateTime.Now ||
                                                                 x.ToDate == null));
                currentSalary.ToDate = employeeSalaryData.FromDate.AddDays(-1);
            }

            var hourlySalaryEntity = new EmployeeHourlySalary
            {
                UserId = employeeSalaryData.UserId,
                FromDateInclusive = employeeSalaryData.FromDate,
                HourlySalary = employeeSalaryData.HourlySalary,
                ToDate = null
            };

            _economyContext.EmployeeHourlySalaries.Add(hourlySalaryEntity);
            _economyContext.SaveChanges();
            
            return ToEmployeeSalary(hourlySalaryEntity);
        }

        public List<EmployeeSalaryDto> GetEmployeeSalaryData(int userId)
        {
            var employeeWithSalaryData = _economyContext
                    .EmployeeHourlySalaries.Where(hs => hs.UserId == userId).ToList()
                ;
            if (!employeeWithSalaryData.Any())
                throw new ValidationException("Could not find a registered salary for this user");

            return employeeWithSalaryData.Select(hourlySalary => ToEmployeeSalary(hourlySalary)).ToList();
        }
        private EmployeeSalaryDto ToEmployeeSalary(EmployeeHourlySalary employeeHourlySalary)
        {
            return new(
                employeeHourlySalary.UserId, 
                employeeHourlySalary.HourlySalary, 
                employeeHourlySalary.FromDateInclusive, 
                employeeHourlySalary.ToDate, 
                employeeHourlySalary.Id);
        }
    }
}