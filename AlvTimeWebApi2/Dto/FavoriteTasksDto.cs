using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlvTimeWebApi2.Dto
{
    public class FavoriteTasksDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }
    }
}
