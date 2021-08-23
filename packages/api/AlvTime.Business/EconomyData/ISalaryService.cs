using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;

namespace AlvTime.Business.EconomyData
{
    public interface ISalaryService
    {
        void RegisterHourlySalary(EmployeeSalaryDto employeeSalaryData);
        List<EmployeeSalaryDto> GetEmployeeSalaryData(int userId);
        OvertimePayoutResponsDto DeleteOvertimePayout(int userId, int paidOvertimeId);
        void SaveOvertimePayout(RegisterOvertimePayoutDto overtimePayout);
    }
}
