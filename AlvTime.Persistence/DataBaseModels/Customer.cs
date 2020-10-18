using System.Collections.Generic;

namespace AlvTime.Persistence.DataBaseModels
{
    public partial class Customer
    {
        public Customer()
        {
            Project = new HashSet<Project>();
        }

        public int Id { get; set; }
        public string Name { get; set; }
        public string InvoiceAddress { get; set; }
        public string ContactPerson { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }

        public virtual ICollection<Project> Project { get; set; }
    }
}
