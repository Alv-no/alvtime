using System;
using System.Collections.Generic;
using AlvTime.Business.EconomyData;
using AlvTime.Business.FlexiHours;

namespace AlvTime.Business.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly IOvertimePayoutStorage _overtimePayoutStorage;
        private readonly IEmployeeHourlySalaryStorage _employeeHourlySalaryStorage;

        public SalaryService(IOvertimePayoutStorage overtimePayoutStorage,
            IEmployeeHourlySalaryStorage employeeHourlySalaryStorage)
        {
            _overtimePayoutStorage = overtimePayoutStorage;
            _employeeHourlySalaryStorage = employeeHourlySalaryStorage;
        }

        public decimal RegisterOvertimePayout(List<OvertimeEntry> overtimeEntries, int userId,
            GenericHourEntry requestedPayout)
        {
            
            var overtimeEntriesForPayout = _overtimePayoutStorage.GetOvertimeEntriesForPayout(overtimeEntries, requestedPayout.Hours);
            var overtimeSalary =_employeeHourlySalaryStorage.CalculateOvertimeSalaryPayout(overtimeEntriesForPayout, userId);

            _overtimePayoutStorage.SaveOvertimePayout(new RegisterOvertimePayoutDto
            {
                TotalPayout = overtimeSalary,
                UserId = userId,
                Date = requestedPayout.Date
            });

            return overtimeSalary;
        }

        public OvertimePayoutResponsDto DeleteOvertimePayout(int userId, DateTime date)
        {
            return _overtimePayoutStorage.DeleteOvertimePayout(userId, date);
        }

        public void RegisterHourlySalary(EmployeeSalaryDto employeeSalaryData)
        {
            _employeeHourlySalaryStorage.RegisterHourlySalary(employeeSalaryData);
        }

        public List<EmployeeSalaryDto> GetEmployeeSalaryData(int userId)
        {
            return _employeeHourlySalaryStorage.GetEmployeeSalaryData(userId);
        }
    }
}