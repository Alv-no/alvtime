using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class Task
    {
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [StringLength(300)]
        public string Description { get; set; }
        public int Project { get; set; }
        [Column(TypeName = "decimal(7, 2)")]
        public decimal? HourRate { get; set; }
        public bool Locked { get; set; }
        public bool Favorite { get; set; }

        [ForeignKey("Project")]
        [InverseProperty("Task")]
        public virtual Project ProjectNavigation { get; set; }
    }
}
