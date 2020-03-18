using System;
using System.Collections.Generic;

namespace AlvTimeWebApi2.DatabaseModels
{
    public partial class Customer
    {
        public Customer()
        {
            Project = new HashSet<Project>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Project> Project { get; set; }
    }
}
