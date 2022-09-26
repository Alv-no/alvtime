using System;
using AlvTime.Business.Validators;

namespace AlvTime.Business.FlexiHours
{
    public class GenericHourEntry
    {
        public DateTime Date { get; set; }
        
        [QuarterHour]
        public decimal Hours { get; set; }
    }
}
