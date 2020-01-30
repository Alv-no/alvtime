using System;
using System.Collections.Generic;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Project { get; set; }
        public decimal? HourRate { get; set; }
        public bool Favorite { get; set; }

        public virtual Project ProjectNavigation { get; set; }
    }
}
