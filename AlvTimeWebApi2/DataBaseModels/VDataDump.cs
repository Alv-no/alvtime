using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class VDataDump
    {
        [Column("taskID")]
        public int TaskId { get; set; }
        [Required]
        [Column("taskName")]
        [StringLength(100)]
        public string TaskName { get; set; }
        [Column("projectID")]
        public int ProjectId { get; set; }
        [Column("value", TypeName = "decimal(6, 2)")]
        public decimal Value { get; set; }
        [Column("userID")]
        public int UserId { get; set; }
        [Column("date", TypeName = "datetime")]
        public DateTime Date { get; set; }
        [Column("userName")]
        [StringLength(100)]
        public string UserName { get; set; }
        [Column("email")]
        [StringLength(100)]
        public string Email { get; set; }
        [Required]
        [Column("projectName")]
        [StringLength(100)]
        public string ProjectName { get; set; }
        [Required]
        [Column("customerName")]
        [StringLength(100)]
        public string CustomerName { get; set; }
        [Column("customerId")]
        public int CustomerId { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal HourRate { get; set; }
    }
}
