using System;

namespace AlvTimeWebApi.Dto
{
    public class HourRateResponseDto
    {
        public int Id { get; set; }
        public string FromDate { get; set; }
        public decimal Rate { get; set; }
        public TaskResponseDto Task { get; set; }
    }
}
