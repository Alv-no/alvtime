using System;
using System.Collections.Generic;

namespace AlvTime.Persistence.DatabaseModels
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
