using System;
using System.Collections.Generic;

namespace AlvTime.Persistence.DatabaseModels
{
    public partial class EmploymentRate
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Rate { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public virtual User User { get; set; }
    }
}
