using System;
using System.Collections.Generic;

namespace AlvTimeWebApi2.DataBaseModels
{
    public partial class TaskFavorites
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TaskId { get; set; }
    }
}
