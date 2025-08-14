namespace AlvTime.Business.Tasks
{
    public record UpdateTaskDto(int Id, bool Favorite = false, bool EnableComments = false);
}
