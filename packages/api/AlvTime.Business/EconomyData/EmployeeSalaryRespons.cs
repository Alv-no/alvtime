using System;

namespace AlvTime.Business.EconomyData
{
    public record EmployeeSalaryRespons 
    {
        public int Id { get; init; }
        public int UsiderId { get; init; }
        public decimal HourlySalary { get; init; }
        public string FromDate { get; init; }
        public string ToDate { get; init; }
    }
}
