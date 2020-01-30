namespace AlvTimeWebApi2.Dto
{
    public class ProjectDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public CustomerDto Customer { get; set; }
    }
}
