using System;

namespace AlvTime.Business.EconomyData
{
    public class EmployeeSalaryDto
    {
        public int usiderId { get; set; }
        public decimal HourlySalary { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}