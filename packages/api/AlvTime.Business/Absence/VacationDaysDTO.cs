using System.Collections.Generic;
using AlvTime.Business.TimeEntries;

namespace AlvTime.Business.Absence {
    public class VacationDaysDTO
    {
        public int AvailableVacationDays { get; set; }    
        public int PlannedVacationDaysThisYear { get; set; }    
        public int UsedVacationDaysThisYear { get; set; }    
        public IEnumerable<TimeEntryResponseDto> PlannedTransactions { get; set; }
        public IEnumerable<TimeEntryResponseDto> UsedTransactions { get; set; }
    }
}
