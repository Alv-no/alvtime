using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class Task
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
        [Required]
        [StringLength(300)]
        public string Description { get; set; }
        public int Project { get; set; }
        public bool Locked { get; set; }
        public bool Favorite { get; set; }

        [ForeignKey(nameof(Project))]
        [InverseProperty("Task")]
        public virtual Project ProjectNavigation { get; set; }
    }
}
