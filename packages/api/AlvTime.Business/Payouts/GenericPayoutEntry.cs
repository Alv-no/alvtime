using System;

namespace AlvTime.Business.Payouts
{
    public class GenericPayoutEntry
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal HoursBeforeCompRate { get; set; }
        public decimal HoursAfterCompRate { get; set; }
        public bool Active { get; set; }
        public decimal CompRate { get; set; }
    }
}
