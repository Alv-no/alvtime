using System;
using System.Collections.Generic;

#nullable disable

namespace AlvTime.Persistence.EconomyDataDBModels
{
    public partial class EmployeeHourlySalary
    {
        public int UserId { get; set; }
        public decimal HourlySalary { get; set; }
        public DateTime FromDateInclusive { get; set; }
        public DateTime? ToDate { get; set; }
        public int Id { get; set; }
    }
}
