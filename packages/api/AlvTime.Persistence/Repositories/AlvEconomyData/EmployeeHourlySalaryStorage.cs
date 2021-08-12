using System;
using System.Linq;
using AlvTime.Business.EconomyData;
using AlvTime.Persistence.EconomyDataDBModels;

namespace AlvTime.Persistence.Repositories.AlvEconomyData
{
    public class EmployeeHourlySalaryStorage : IEmployeeHourlySalaryStorage
    {
        private readonly AlvEconomyDataContext _economyContext;

        public EmployeeHourlySalaryStorage(AlvEconomyDataContext economyContext)
        {
            _economyContext = economyContext;
        }


        public decimal GetHouerlySalary(int userId, DateTime date)
        {
            var employeeWithSalaryData = _economyContext
                .EmployeeHourlySalaries.Where(hs => hs.UserId == userId)
                .OrderBy(hs => hs.FromDateInclusive);

            var salary = employeeWithSalaryData.FirstOrDefault(x => x.FromDateInclusive <= date && ((x.ToDate.HasValue && x.ToDate > date) || (x.ToDate == null)));
            
            return salary.HourlySalary;
        }
    }
}