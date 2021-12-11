using System;
using System.Collections.Generic;

#nullable disable

namespace AlvTime.Persistence.DataBaseModels
{
    public partial class HourRate
    {
        public DateTime FromDate { get; set; }
        public decimal Rate { get; set; }
        public int TaskId { get; set; }
        public int Id { get; set; }

        public virtual Task Task { get; set; }
    }
}
