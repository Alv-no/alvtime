using AlvTime.Business.Validators;
using System;
using System.ComponentModel.DataAnnotations;

namespace AlvTime.Business.TimeEntries
{
    public class CreateTimeEntryDto
    {
        public DateTime Date { get; set; }

        [QuarterHour]
        [Range(0, int.MaxValue, ErrorMessage = "Only positive values allowed")]
        public decimal Value { get; set; }

        public int TaskId { get; set; }
    }
}
