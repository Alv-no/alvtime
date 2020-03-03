using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class Project
    {
        public Project()
        {
            Task = new HashSet<Task>();
        }

        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        public int? Customer { get; set; }

        [ForeignKey(nameof(Customer))]
        [InverseProperty("Project")]
        public virtual Customer CustomerNavigation { get; set; }
        [InverseProperty("ProjectNavigation")]
        public virtual ICollection<Task> Task { get; set; }
    }
}
