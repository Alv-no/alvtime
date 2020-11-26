using System;

namespace AlvTime.Business.FlexiHours
{
    public class GenericPayoutEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Hours { get; set; }
        public bool Active { get; set; }
    }
}
