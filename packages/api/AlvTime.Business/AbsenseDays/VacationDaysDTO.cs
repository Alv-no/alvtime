using System.Collections.Generic;
using AlvTime.Business.TimeEntries;

namespace AlvTime.Business.AbsenseDays {
    public class VacationDaysDTO
    {
        public int AvailableVacationDays { get; set; }    
        public int PlannedVacationDays { get; set; }    
        public int UsedVacationDays { get; set; }    
        public IEnumerable<TimeEntriesResponseDto> PlannedTransactions { get; set; }
        public IEnumerable<TimeEntriesResponseDto> UsedTransactions { get; set; }
    }
}
