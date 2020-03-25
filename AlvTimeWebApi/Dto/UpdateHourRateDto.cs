using System;

namespace AlvTimeWebApi.Dto
{
    public class UpdateHourRateDto
    {
        public DateTime FromDate { get; set; }
        public decimal Rate { get; set; }
        public int TaskId { get; set; }
        public int Id { get; set; }
    }
}
