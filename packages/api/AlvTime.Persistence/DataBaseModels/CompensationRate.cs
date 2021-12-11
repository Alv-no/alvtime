using System;
using System.Collections.Generic;

#nullable disable

namespace AlvTime.Persistence.DataBaseModels
{
    public partial class CompensationRate
    {
        public int Id { get; set; }
        public DateTime FromDate { get; set; }
        public decimal Value { get; set; }
        public int TaskId { get; set; }

        public virtual Task Task { get; set; }
    }
}
