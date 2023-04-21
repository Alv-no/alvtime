using System.Collections.Generic;
using AlvTime.Business.TimeEntries;

namespace AlvTime.Business.Absence {
    public class VacationDaysDTO
    {
        public decimal AvailableVacationDays { get; set; }
        public decimal AvailableVacationDaysTransferedFromLastYear { get; set; }
        public decimal PlannedVacationDaysThisYear { get; set; }
        public decimal UsedVacationDaysThisYear { get; set; }
        public IEnumerable<TimeEntryResponseDto> PlannedTransactions { get; set; }
        public IEnumerable<TimeEntryResponseDto> UsedTransactions { get; set; }
    }
}
