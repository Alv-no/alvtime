using System;
using AlvTime.Business.Overtime;

namespace AlvTime.Persistence.DatabaseModels
{
    public class SalaryModelHistory
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime SwitchDate { get; set; }
        public SalaryModel PreviousModel { get; set; }
        public SalaryModel NewModel { get; set; }

        public virtual User User { get; set; }
    }
}
