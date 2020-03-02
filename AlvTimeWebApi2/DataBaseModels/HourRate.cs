using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class HourRate
    {
        public DateTime FromDate { get; set; }
        [Column(TypeName = "decimal(10, 2)")]
        public decimal Rate { get; set; }
        public int TaskId { get; set; }
        public int Id { get; set; }
    }
}
