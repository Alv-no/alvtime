using System;
using System.Collections.Generic;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class Hours
    {
        public int Id { get; set; }
        public int User { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
        public short DayNumber { get; set; }
        public short Year { get; set; }
        public int TaskId { get; set; }
        public bool Locked { get; set; }
    }
}
