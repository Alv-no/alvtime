using AlvTime.Business.Projects;

namespace AlvTimeWebApi.Responses
{
    public record TaskResponse(int Id, string Name, string Description, bool Favorite, bool Locked,
        decimal CompensationRate, ProjectResponseDto Project);
}