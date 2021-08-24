using System;

namespace AlvTime.Business.EconomyData
{
    public record EmployeeSalaryRequest
    {
        public int UsiderId { get; init; }
        public decimal HourlySalary { get; init; }
        public DateTime FromDate { get; init; }
        public DateTime? ToDate { get; init; }
    }
}
