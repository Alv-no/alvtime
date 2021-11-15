namespace AlvTimeWebApi.Requests
{
    public record TaskUpdateRequest(int Id, bool? Locked, string Name, decimal? CompensationRate, bool Favorite = false);
}