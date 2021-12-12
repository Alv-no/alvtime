using System;

namespace AlvTime.Business.Overtime
{
    public class OvertimeEntry
    {
        public DateTime Date { get; set; }
        public decimal Hours { get; set; }
        public int TaskId { get; set; }
        public decimal CompensationRate { get; set; }
    }
}
