namespace AlvTimeWebApi.Requests
{
    public record TaskCreateRequest(string Name, string Description, int Project, bool Locked,
        decimal CompensationRate = 1.0M);
}