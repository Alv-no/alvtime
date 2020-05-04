using System;

namespace AlvTime.Business.TimeEntries
{
    public class TimeEntriesResponseDto
    {
        public int User { get; set; }
        public int Id { get; set; }
        public string Date { get; set; }
        public decimal Value { get; set; }
        public int TaskId { get; set; }
    }
}
