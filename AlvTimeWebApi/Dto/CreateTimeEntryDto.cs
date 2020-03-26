using System;

namespace AlvTimeWebApi.Dto
{
    public class CreateTimeEntryDto
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public int TaskId { get; set; }
    }
}
