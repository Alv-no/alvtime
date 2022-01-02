using System;

namespace AlvTime.Business.TimeEntries
{
    public class TimeEntryResponseDto
    {
        public int User { get; set; }
        public string UserEmail { get; set; }
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public int TaskId { get; set; }
    }
}
