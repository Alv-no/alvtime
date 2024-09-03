﻿using System;
using System.Collections.Generic;

namespace AlvTime.Persistence.DatabaseModels
{
    public partial class Hours
    {
        public int Id { get; set; }
        public int User { get; set; }
        public decimal Value { get; set; }
        public DateTime Date { get; set; }
        public short DayNumber { get; set; }
        public short Year { get; set; }
        public int TaskId { get; set; }
        public bool Locked { get; set; }
        public DateTime? TimeRegistered { get; set; }

        public string? Comment { get; set; }

        public DateTime? CommentedAt { get; set; }

        public virtual Task Task { get; set; }
        public virtual User UserNavigation { get; set; }
    }
}
