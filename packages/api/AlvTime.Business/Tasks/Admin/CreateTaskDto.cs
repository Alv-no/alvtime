namespace AlvTime.Business.Tasks.Admin
{
    public class CreateTaskDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Project { get; set; }
        public bool Locked { get; set; }
        public decimal CompensationRate { get; set; } = 1.0M;
    }
}
