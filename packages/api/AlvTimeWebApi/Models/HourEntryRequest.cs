using System;
using System.ComponentModel.DataAnnotations;
using AlvTime.Business.Validators;

namespace AlvTimeWebApi.Models
{
    public class HourEntryRequest
    {
        public DateTime Date { get; set; }

        [HalfHour]
        [Range(0, int.MaxValue, ErrorMessage = "Only positive values allowed")]
        public decimal Hours { get; set; }
    }
}
