using AlvTimeWebApi.Dto;

namespace AlvTimeWebApi.Dto
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Favorite { get; set; }
        public bool Locked { get; set; }

        public ProjectDto Project { get; set; }
    }
}