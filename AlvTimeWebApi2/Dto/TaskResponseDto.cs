using AlvTimeApi.DataBaseModels;

namespace AlvTimeApi.Controllers.Tasks
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Project Project { get; set; }
        public bool Favorite { get; set; }
        public bool Locked { get; set; }
        public double HourRate { get; set; }
    }
}
