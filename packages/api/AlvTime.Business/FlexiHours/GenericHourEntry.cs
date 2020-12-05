using AlvTime.Business.Validators;
using System;
using System.ComponentModel.DataAnnotations;

namespace AlvTime.Business.FlexiHours
{
    public class GenericHourEntry
    {
        public DateTime Date { get; set; }

        [HalfHour]
        [Range(0, int.MaxValue, ErrorMessage = "Only positive values allowed")]
        public decimal Hours { get; set; }
    }
}
