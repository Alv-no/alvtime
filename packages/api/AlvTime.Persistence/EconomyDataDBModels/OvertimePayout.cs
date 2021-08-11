using System;
using System.Collections.Generic;

#nullable disable

namespace AlvTime.Persistence.EconomyDataDBModels
{
    public partial class OvertimePayout
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalPayout { get; set; }
    }
}
