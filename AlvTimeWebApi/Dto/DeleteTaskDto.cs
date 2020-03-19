namespace AlvTimeWebApi.Dto
{
    public class DeleteTaskDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Project { get; set; }
        public bool Locked { get; set; }
    }
}
