using System;
using AlvTime.Business.Validators;

namespace AlvTime.Business.Payouts
{
    public class GenericPayoutHourEntry
    {
        public DateTime Date { get; set; }
        
        public decimal Hours { get; set; }
    }
}
