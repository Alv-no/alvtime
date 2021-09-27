using System;

namespace AlvTime.Business.EconomyData
{
    public record EmployeeSalaryRequest (int UserId, decimal HourlySalary,  DateTime FromDate)
    {
    }
}
