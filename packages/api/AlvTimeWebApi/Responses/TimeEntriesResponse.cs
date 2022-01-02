using System.Collections.Generic;

namespace AlvTimeWebApi.Responses
{
    public class TimeEntriesResponse
    {
        public decimal TotalHours { get; set; }
        public List<GenericTimeEntryResponse> Entries { get; set; }
    }
}