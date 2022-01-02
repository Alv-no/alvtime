using System.Collections.Generic;
using AlvTime.Business.TimeEntries;

namespace AlvTime.Business.AbsenseDays {
    public class VacationDaysDTO
    {
        public int AvailableVacationDays { get; set; }    
        public int PlannedVacationDays { get; set; }    
        public int UsedVacationDays { get; set; }    
        public IEnumerable<TimeEntryResponseDto> PlannedTransactions { get; set; }
        public IEnumerable<TimeEntryResponseDto> UsedTransactions { get; set; }
    }
}
