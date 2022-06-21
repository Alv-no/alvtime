using System;
using System.Collections.Generic;

namespace AlvTime.Persistence.DatabaseModels
{
    public partial class User
    {
        public User()
        {
            AccessTokens = new HashSet<AccessTokens>();
            AssociatedTasks = new HashSet<AssociatedTasks>();
            EarnedOvertime = new HashSet<EarnedOvertime>();
            Hours = new HashSet<Hours>();
            PaidOvertime = new HashSet<PaidOvertime>();
            TaskFavorites = new HashSet<TaskFavorites>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public virtual ICollection<AccessTokens> AccessTokens { get; set; }
        public virtual ICollection<AssociatedTasks> AssociatedTasks { get; set; }
        public virtual ICollection<EarnedOvertime> EarnedOvertime { get; set; }
        public virtual ICollection<Hours> Hours { get; set; }
        public virtual ICollection<PaidOvertime> PaidOvertime { get; set; }
        public virtual ICollection<TaskFavorites> TaskFavorites { get; set; }
    }
}
