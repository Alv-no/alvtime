namespace AlvTime.Business.Tasks.Admin
{
    public record CreateTaskDto(string Name, string Description, int Project, bool Locked,
        decimal CompensationRate = 1.0M);
}
