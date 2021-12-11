using System;
using System.Collections.Generic;

#nullable disable

namespace AlvTime.Persistence.DataBaseModels
{
    public partial class EarnedOvertime
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
        public decimal CompensationRate { get; set; }

        public virtual User User { get; set; }
    }
}
