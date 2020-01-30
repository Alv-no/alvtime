using AlvTimeWebApi2.DataBaseModels;

namespace AlvTimeApi.Controllers.Tasks
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? HourRate { get; set; }
        public bool Favorite { get; set; }
        public bool Locked { get; set; }

        public virtual Project ProjectNavigation { get; set; }
    }
}
