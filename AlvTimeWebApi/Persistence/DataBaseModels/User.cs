using System;
using System.Collections.Generic;

namespace AlvTimeWebApi.Persistence.DatabaseModels
{
    public partial class User
    {
        public User()
        {
            AccessTokens = new HashSet<AccessTokens>();
            Hours = new HashSet<Hours>();
            TaskFavorites = new HashSet<TaskFavorites>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public decimal FlexiHours { get; set; }
        public DateTime StartDate { get; set; }

        public virtual ICollection<AccessTokens> AccessTokens { get; set; }
        public virtual ICollection<Hours> Hours { get; set; }
        public virtual ICollection<TaskFavorites> TaskFavorites { get; set; }
    }
}
