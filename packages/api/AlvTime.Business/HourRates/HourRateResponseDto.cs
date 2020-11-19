using AlvTime.Business.Tasks;
using System;

namespace AlvTime.Business.HourRates
{
    public class HourRateResponseDto
    {
        public int Id { get; set; }
        public string FromDate { get; set; }
        public decimal Rate { get; set; }
        public TaskResponseDto Task { get; set; }
    }
}
