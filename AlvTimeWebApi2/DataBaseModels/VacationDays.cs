using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlvTimeApi.DataBaseModels
{
    public class VacationDays
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int AvailableDays { get; set; }
        public int UsedDays { get; set; }
    }
}
