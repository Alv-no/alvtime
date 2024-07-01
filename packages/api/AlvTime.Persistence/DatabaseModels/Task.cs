using System.Collections.Generic;

namespace AlvTime.Persistence.DatabaseModels
{
    public partial class Task
    {
        public Task()
        {
            AssociatedTasks = new HashSet<AssociatedTasks>();
            CompensationRate = new HashSet<CompensationRate>();
            HourRate = new HashSet<HourRate>();
            Hours = new HashSet<Hours>();
            TaskFavorites = new HashSet<TaskFavorites>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Project { get; set; }
        public bool Locked { get; set; }
        public bool Favorite { get; set; }
        public bool Imposed { get; set; }

        public virtual Project ProjectNavigation { get; set; }
        public virtual ICollection<AssociatedTasks> AssociatedTasks { get; set; }
        public virtual ICollection<CompensationRate> CompensationRate { get; set; }
        public virtual ICollection<HourRate> HourRate { get; set; }
        public virtual ICollection<Hours> Hours { get; set; }
        public virtual ICollection<TaskFavorites> TaskFavorites { get; set; }
    }
}
