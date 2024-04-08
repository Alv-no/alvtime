using AlvTime.Business.Tasks;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses.Admin;

namespace AlvTimeWebApi.Controllers.Utils;

public static class TaskMapper
{
    public static TaskDto MapToTaskDto(this TaskUpsertRequest taskUpsertRequest)
    {
        return new TaskDto
        {
            Name = taskUpsertRequest.Name,
            Description = taskUpsertRequest.Description,
            Locked = taskUpsertRequest.Locked,
            CompensationRate = taskUpsertRequest.CompensationRate,
            Imposed = taskUpsertRequest.Imposed
        };
    }
    
    public static TaskDto MapToTaskDto(this TaskUpsertRequest taskUpsertRequest, int taskId)
    {
        return new TaskDto
        {
            Id = taskId,
            Name = taskUpsertRequest.Name,
            Description = taskUpsertRequest.Description,
            Locked = taskUpsertRequest.Locked,
            CompensationRate = taskUpsertRequest.CompensationRate,
            Imposed = taskUpsertRequest.Imposed
        };
    }
    
    public static TaskResponseSimple MapToTaskResponseSimple(this TaskDto taskDto)
    {
        return new TaskResponseSimple
        {
            Id = taskDto.Id,
            Name = taskDto.Name,
            Description = taskDto.Description,
            Favorite = false,
            Locked = taskDto.Locked,
            CompensationRate = taskDto.CompensationRate
        };
    }
}