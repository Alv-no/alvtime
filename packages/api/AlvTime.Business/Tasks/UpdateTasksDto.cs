namespace AlvTime.Business.Tasks
{
    public class UpdateTasksDto
    {
        public int Id { get; set; }
        public bool Favorite { get; set; }
        public bool? Locked { get; set; }
        public string Name { get; set; }
        public decimal? CompensationRate { get; set; }
    }
}
