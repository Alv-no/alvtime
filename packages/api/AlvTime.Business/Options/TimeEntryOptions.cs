using System;

namespace AlvTime.Business.Options
{
    public class TimeEntryOptions
    {
        public int FlexTask { get; set; }
        public DateTime StartOfOvertimeSystem { get; set; }
        public int ReportUser { get; set; }
        public int PaidHolidayTask { get; set; }
        public int UnpaidHolidayTask { get; set; }
        public int SickDaysTask { get; set; }
        public int AlvDayTask { get; set; }
    }
}
