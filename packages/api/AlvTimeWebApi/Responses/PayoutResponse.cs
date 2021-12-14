using System;

namespace AlvTimeWebApi.Responses
{
    public class PayoutResponse
    {
        public int Id { get; set; }
        public String Date { get; set; }
        public decimal HoursBeforeCompRate { get; set; }
        public decimal HoursAfterCompRate { get; set; }
        public bool Active { get; set; }
    }
}