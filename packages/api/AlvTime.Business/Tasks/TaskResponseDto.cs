using AlvTime.Business.Projects;

namespace AlvTime.Business.Tasks
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Favorite { get; set; }
        public bool Locked { get; set; }
        public decimal CompensationRate { get; set; }
        public bool Imposed { get; set; }

        public ProjectResponseDto Project { get; set; }
    }
}