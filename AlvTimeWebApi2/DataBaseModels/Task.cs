using AlvTimeApi.DataBaseModels;
using System;
using System.Collections.Generic;

namespace TimeTracker1.Models
{
    public class Task
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Project { get; set; }
        public bool Favorite { get; set; }
        public bool Locked { get; set; }
        public string Customer { get; set; }
        public double HourRate { get; set; }
    }
}
