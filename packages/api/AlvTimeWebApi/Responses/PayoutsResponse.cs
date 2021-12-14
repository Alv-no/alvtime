using System;
using System.Collections.Generic;

namespace AlvTimeWebApi.Responses
{
    public class PayoutsResponse
    {
        public decimal TotalHoursBeforeCompRate { get; set; }
        public decimal TotalHoursAfterCompRate { get; set; }
        public List<PayoutResponse> Entries { get; set; }
    }


}