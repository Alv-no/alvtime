using System.Collections.Generic;

namespace AlvTime.Persistence.DataBaseModels
{
    public partial class Project
    {
        public Project()
        {
            Task = new HashSet<Task>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public int Customer { get; set; }

        public virtual Customer CustomerNavigation { get; set; }
        public virtual ICollection<Task> Task { get; set; }
    }
}
