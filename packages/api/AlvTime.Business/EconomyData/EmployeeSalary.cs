using System;

namespace AlvTime.Business.EconomyData
{
    public record EmployeeSalary(int UserId, decimal HourlySalary, DateTime FromDate, DateTime? ToDate, int Id) : 
        EmployeeSalaryRequest(UserId, HourlySalary, FromDate, ToDate)
    {
    }
}