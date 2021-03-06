﻿using System;

namespace AlvTime.Persistence.DataBaseModels
{
    public partial class PaidOvertime
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int User { get; set; }
        public decimal HoursBeforeCompRate { get; set; }
        public decimal HoursAfterCompRate { get; set; }

        public virtual User UserNavigation { get; set; }
    }
}
