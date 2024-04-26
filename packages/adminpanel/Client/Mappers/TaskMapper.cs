using Alvtime.Adminpanel.Client.Models;
using Alvtime.Adminpanel.Client.Requests;

namespace Alvtime.Adminpanel.Client.Mappers;

public static class TaskMapper
{
    public static TaskUpsertRequest MapToTaskUpsertRequest(this TaskModel task)
    {
        return new TaskUpsertRequest
        {
            Name = task.Name,
            Description = task.Description,
            Locked = task.Locked,
            CompensationRate = task.CompensationRate,
            Imposed = task.Imposed
        };
    }
}