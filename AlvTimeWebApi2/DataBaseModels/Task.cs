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
        public Project Project { get; set; }
        public bool Favorite { get; set; }
        public bool Locked { get; set; }
        public Customer Customer { get; set; }
        public double HourRate { get; set; }
    }
}
