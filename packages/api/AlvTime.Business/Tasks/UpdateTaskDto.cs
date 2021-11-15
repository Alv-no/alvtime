namespace AlvTime.Business.Tasks
{
    public record UpdateTaskDto(int Id, bool? Locked, string Name, decimal? CompensationRate, bool Favorite = false);
}
