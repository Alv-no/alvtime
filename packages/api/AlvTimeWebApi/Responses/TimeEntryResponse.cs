using System;
using AlvTime.Business.Overtime;

namespace AlvTimeWebApi.Responses
{
    public class TimeEntryResponse
    {
        public String Date { get; set; }
        public decimal Hours { get; set; }
        public decimal CompensationRate { get; set; }
        public TimeEntryType Type { get; set; } = TimeEntryType.Overtime;
        public bool? Active { get; set; }
    }
}