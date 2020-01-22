using AlvTimeApi.DataBaseModels;

namespace TimeTracker1.Dto
{
    public class WorkHourDto
    {
        public string TaskName { get; set; }
        public decimal Monday { get; set; }
        public Hours MondayObject { get; set; }
        public decimal Tuesday { get; set; }
        public Hours TuesdayObject { get; set; }
        public decimal Wednesday { get; set; }
        public Hours WednesdayObject { get; set; }
        public decimal Thursday { get; set; }
        public Hours ThursdayObject { get; set; }
        public decimal Friday { get; set; }
        public Hours FridayObject { get; set; }
        public decimal Saturday { get; set; }
        public Hours SaturdayObject { get; set; }
        public decimal Sunday { get; set; }
        public Hours SundayObject { get; set; }
    }
}
