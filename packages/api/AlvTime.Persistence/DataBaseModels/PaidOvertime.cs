using System;

namespace AlvTime.Persistence.DataBaseModels
{
    public partial class PaidOvertime
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public int User { get; set; }
        public decimal Value { get; set; }

        public virtual User UserNavigation { get; set; }
    }
}
