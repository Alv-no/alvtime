using System;

namespace AlvTimeApi.Controllers.Tasks
{
    public class TimeEntriesResponseDto
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public int TaskId { get; set; }
    }
}
