using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class Hours
    {
        public int Id { get; set; }
        public int User { get; set; }
        [Column(TypeName = "decimal(6, 2)")]
        public decimal Value { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime Date { get; set; }
        public short DayNumber { get; set; }
        public short Year { get; set; }
        public int TaskId { get; set; }
        public bool Locked { get; set; }
    }
}
