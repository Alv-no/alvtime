using System;

namespace AlvTimeApi.Dto
{
    public class SaveHoursDto
    {
        public DateTime Date { get; set; }
        public decimal Value { get; set; }
        public int TaskId { get; set; }
    }
}
