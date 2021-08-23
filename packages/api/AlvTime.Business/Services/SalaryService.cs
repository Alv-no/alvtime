using System.Collections.Generic;
using AlvTime.Business.EconomyData;

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
        
        public OvertimePayoutResponsDto DeleteOvertimePayout(int userId, int paidOvertimeId)
        {
            return _overtimePayoutStorage.DeleteOvertimePayout(userId, paidOvertimeId);
        }

        public void SaveOvertimePayout(RegisterOvertimePayoutDto overtimePayout)
        {
            _overtimePayoutStorage.SaveOvertimePayout(overtimePayout);
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