using System.Collections.Generic;
using AlvTime.Business.EconomyData;

namespace AlvTime.Business.Services
{
    public class SalaryService : ISalaryService
    {
        private readonly IEmployeeHourlySalaryStorage _employeeHourlySalaryStorage;
        private readonly IOvertimePayoutStorage _overtimePayoutStorage;

        public SalaryService(IOvertimePayoutStorage overtimePayoutStorage,
            IEmployeeHourlySalaryStorage employeeHourlySalaryStorage)
        {
            _overtimePayoutStorage = overtimePayoutStorage;
            _employeeHourlySalaryStorage = employeeHourlySalaryStorage;
        }

        public OvertimePayoutDto DeleteOvertimePayout(int userId, int paidOvertimeId)
        {
            return _overtimePayoutStorage.DeleteOvertimePayout(userId, paidOvertimeId);
        }

        public OvertimePayoutDto SaveOvertimePayout(RegisterOvertimePayout overtimePayout)
        {
            return _overtimePayoutStorage.SaveOvertimePayout(overtimePayout);
        }

        public EmployeeSalaryDto RegisterHourlySalary(EmployeeSalaryRequest employeeSalaryData)
        {
            return _employeeHourlySalaryStorage.RegisterHourlySalary(ToEmployeeSalary(employeeSalaryData));
        }

        public List<EmployeeSalaryDto> GetEmployeeSalaryData(int userId)
        {
            return _employeeHourlySalaryStorage.GetEmployeeSalaryData(userId);
        }

        public EmployeeSalaryRequest ToEmployeeSalary(EmployeeSalaryRequest employeeSalaryRequest)
        {
            return new(
                employeeSalaryRequest.UserId,
                employeeSalaryRequest.HourlySalary,
                employeeSalaryRequest.FromDate,
                employeeSalaryRequest.ToDate.HasValue ? employeeSalaryRequest.ToDate.Value : null);
        }
    }
}