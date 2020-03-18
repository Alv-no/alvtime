using System;
using System.Collections.Generic;

namespace AlvTimeWebApi2.DatabaseModels
{
    public partial class User
    {
        public User()
        {
            Hours = new HashSet<Hours>();
            TaskFavorites = new HashSet<TaskFavorites>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public virtual ICollection<Hours> Hours { get; set; }
        public virtual ICollection<TaskFavorites> TaskFavorites { get; set; }
    }
}
