using System;
using System.Collections.Generic;

namespace AlvTimeWebApi2.DatabaseModels
{
    public partial class TaskFavorits
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }
    }
}
