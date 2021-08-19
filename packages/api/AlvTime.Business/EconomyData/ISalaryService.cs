using System;
using System.Collections.Generic;
using AlvTime.Business.FlexiHours;

namespace AlvTime.Business.EconomyData
{
    public interface ISalaryService
    {
        decimal RegisterOvertimePayout(List<OvertimeEntry> overtimeEntries, int userId, GenericHourEntry requestedPayout);
        void RegisterHourlySalary(EmployeeSalaryDto employeeSalaryData);
        List<EmployeeSalaryDto> GetEmployeeSalaryData(int userId);
        OvertimePayoutResponsDto DeleteOvertimePayout(int userId, DateTime date);
    }
}
