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
        
        public OvertimePayoutRespons DeleteOvertimePayout(int userId, int paidOvertimeId)
        {
            return _overtimePayoutStorage.DeleteOvertimePayout(userId, paidOvertimeId);
        }

        public void SaveOvertimePayout(RegisterOvertimePayout overtimePayout)
        {
            _overtimePayoutStorage.SaveOvertimePayout(overtimePayout);
        }

        public EmployeeSalary RegisterHourlySalary(EmployeeSalaryRequest employeeSalaryData)
        {
            return _employeeHourlySalaryStorage.RegisterHourlySalary( ToEmployeeSalary( employeeSalaryData));
        }

        public List<EmployeeSalary> GetEmployeeSalaryData(int userId)
        {
            return _employeeHourlySalaryStorage.GetEmployeeSalaryData(userId);
        }

        public EmployeeSalary ToEmployeeSalary(EmployeeSalaryRequest employeeSalaryRequest)
        {
            return new EmployeeSalary
            {
                UsiderId = employeeSalaryRequest.UsiderId,
                FromDate = employeeSalaryRequest.FromDate,
                ToDate = employeeSalaryRequest.ToDate.HasValue ? employeeSalaryRequest.ToDate.Value : null,
                HourlySalary = employeeSalaryRequest.HourlySalary
            };
        }
    }
}