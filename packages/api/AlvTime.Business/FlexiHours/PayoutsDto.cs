using System.Collections.Generic;

namespace AlvTime.Business.FlexiHours
{
    public class PayoutsDto
    {
        public decimal TotalHours { get; set; }
        public List<GenericPayoutEntry> Entries { get; set; }
    }
}
