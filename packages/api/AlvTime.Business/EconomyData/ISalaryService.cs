using System.Collections.Generic;

namespace AlvTime.Business.EconomyData
{
    public interface ISalaryService
    {
        EmployeeSalaryDto RegisterHourlySalary(EmployeeSalaryRequest employeeSalaryData);
        List<EmployeeSalaryDto> GetEmployeeSalaryData(int userId);
        OvertimePayoutDto DeleteOvertimePayout(int userId, int paidOvertimeId);
        OvertimePayoutDto SaveOvertimePayout(RegisterOvertimePayout overtimePayout);
    }
}
