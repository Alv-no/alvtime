using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TimeTracker1.Models;

namespace AlvTimeApi.Dto
{
    public class UpdateTasksDto
    {
        public int Id { get; set; }
        public bool Favorite { get; set; }
    }
}
