using AlvTime.Persistence.DatabaseModels;
using System;

namespace AlvTime.Persistence.DataBaseModels
{
    public partial class AssociatedTasks
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime EndDate { get; set; }

        public virtual Task Task { get; set; }
        public virtual User User { get; set; }
    }
}
