using System.Collections.Generic;

namespace AlvTime.Business.EconomyData
{
    public interface IEmployeeHourlySalaryStorage
    {
        EmployeeSalaryDto RegisterHourlySalary(EmployeeSalaryDto employeeSalaryData);
        List<EmployeeSalaryDto> GetEmployeeSalaryData(int userId);
    }
}
