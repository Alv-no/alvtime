using System;
using System.Collections.Generic;

namespace AlvTime.Persistence.DatabaseModels
{
    public partial class PaidOvertime
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public bool IsLocked { get; set; }
        public int User { get; set; }
        public decimal HoursBeforeCompRate { get; set; }
        public decimal HoursAfterCompRate { get; set; }
        public decimal CompensationRate { get; set; }
        public virtual User UserNavigation { get; set; }
    }
}
