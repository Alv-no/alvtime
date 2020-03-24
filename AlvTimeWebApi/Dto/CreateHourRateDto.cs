using System;

namespace AlvTimeWebApi.Dto
{
    public class CreateHourRateDto
    {
        public DateTime FromDate { get; set; }
        public decimal Rate { get; set; }
        public int TaskId { get; set; }
    }
}
