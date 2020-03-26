using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlvTimeWebApi.Dto
{
    public class CreateProjectDto
    {
        public string Name { get; set; }
        public int Customer { get; set; }
    }
}
