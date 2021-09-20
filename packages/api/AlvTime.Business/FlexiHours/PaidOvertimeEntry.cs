using System;

namespace AlvTime.Business.FlexiHours
{
    public class PaidOvertimeEntry
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal HoursBeforeCompensation { get; set; }
        public decimal HoursAfterCompensation { get; set; }
    }
}
