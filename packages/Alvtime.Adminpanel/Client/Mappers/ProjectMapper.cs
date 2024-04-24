using Alvtime.Adminpanel.Client.Models;
using Alvtime.Adminpanel.Client.Requests;

namespace Alvtime.Adminpanel.Client.Mappers;

public static class ProjectMapper
{
    public static ProjectUpsertRequest MapToProjectUpsertRequest(this ProjectModel project)
    {
        return new ProjectUpsertRequest
        {
            Name = project.Name
        };
    }
}