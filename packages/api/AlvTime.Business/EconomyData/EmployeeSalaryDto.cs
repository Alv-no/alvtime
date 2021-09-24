using System;

namespace AlvTime.Business.EconomyData
{
    public record EmployeeSalaryDto(int UserId, decimal HourlySalary, DateTime FromDate, DateTime? ToDate, int Id)
    {
    }
}