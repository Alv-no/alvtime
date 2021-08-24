using System.Collections.Generic;

namespace AlvTime.Business.EconomyData
{
    public interface ISalaryService
    {
        EmployeeSalary RegisterHourlySalary(EmployeeSalaryRequest employeeSalaryData);
        List<EmployeeSalary> GetEmployeeSalaryData(int userId);
        OvertimePayoutRespons DeleteOvertimePayout(int userId, int paidOvertimeId);
        void SaveOvertimePayout(RegisterOvertimePayout overtimePayout);
    }
}
