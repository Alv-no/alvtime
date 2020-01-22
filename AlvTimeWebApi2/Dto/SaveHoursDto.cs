using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlvTimeApi.Dto
{
    public class SaveHoursDto
    {
        public DateTime Date { get; set; }
        public int Value { get; set; }
        public int TaskId { get; set; }
    }
}
