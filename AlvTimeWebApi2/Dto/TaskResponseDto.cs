using AlvTimeWebApi2.DataBaseModels;
using AlvTimeWebApi2.Dto;

namespace AlvTimeApi.Controllers.Tasks
{
    public class TaskResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal? HourRate { get; set; }

        public ProjectDto Project { get; set; }
    }
}
