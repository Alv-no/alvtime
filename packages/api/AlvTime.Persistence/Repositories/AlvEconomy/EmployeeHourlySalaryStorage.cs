using System;
using System.Linq;
using AlvTime.Business.EconomyData;
using AlvTime.Persistence.EconomyDataDBModels;

namespace AlvTime.Persistence.Repositories.AlvEconomy
{
    public class EmployeeHourlySalaryStorage: IEmployeeHourlySalaryStorage
    {
        private AlvEconomyDataContext _economyContext;

        public EmployeeHourlySalaryStorage(AlvEconomyDataContext economyContext)
        {
            _economyContext = economyContext;
        }



        public decimal GetHouerlySalary(int userId, DateTime date)
        {
            var employeeWithSalaryData=_economyContext
                .EmployeeHourlySalaries.Where(hs => hs.UserId == userId)
                .OrderBy(hs=>hs.FromDateInclusive);

            var salary = employeeWithSalaryData.FirstOrDefault(x => x.FromDateInclusive <= date);

            return salary.HourlySalary;
        }
    }
}