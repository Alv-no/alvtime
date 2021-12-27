using System;

namespace AlvTimeWebApi.Responses
{
    public class TimeEntryResponse
    {
        public String Date { get; set; }
        public decimal Hours { get; set; }
        public decimal CompensationRate { get; set; }
    }
}