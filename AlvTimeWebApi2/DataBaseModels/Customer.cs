using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class Customer
    {
        public Customer()
        {
            Project = new HashSet<Project>();
        }

        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [InverseProperty("CustomerNavigation")]
        public virtual ICollection<Project> Project { get; set; }
    }
}
