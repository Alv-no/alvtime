using AlvTime.Business.Projects;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;

namespace AlvTimeWebApi.Controllers.Utils;

public static class ProjectMapper
{
    public static ProjectResponse MapToProjectResponse(this ProjectDto project)
    {
        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name
        };
    }
    
    public static ProjectDto MapToProjectDto(this ProjectUpsertRequest project, int projectId)
    {
        return new ProjectDto
        {
            Id = projectId,
            Name = project.Name
        };
    }
}