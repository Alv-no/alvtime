using System.Collections.Generic;

namespace AlvTime.Business.EconomyData
{
    public interface IEmployeeHourlySalaryStorage
    {
        EmployeeSalary RegisterHourlySalary(EmployeeSalaryRequest employeeSalaryData);
        List<EmployeeSalary> GetEmployeeSalaryData(int userId);
    }
}
