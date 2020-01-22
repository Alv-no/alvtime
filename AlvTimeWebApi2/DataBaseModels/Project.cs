using System;
using System.Collections.Generic;

namespace AlvTimeApi.DataBaseModels
{
    public partial class Project
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int? Customer { get; set; }
    }
}
