using System;
using System.Collections.Generic;

namespace AlvTime.Persistence.DatabaseModels
{
    public partial class VacationDaysEarnedOverride
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public decimal DaysEarned { get; set; }
        public int Year { get; set; }

        public virtual User User { get; set; }
    }
}
