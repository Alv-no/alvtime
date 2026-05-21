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
            CompensationType = task.CompensationType,
            Imposed = task.Imposed
        };
    }

    public static string MapCompensationType(CompensationType compensationType) => compensationType switch
    {
        CompensationType.Volunteer => "Frivillig",
        CompensationType.Internal => "Intern",
        CompensationType.Billable => "Fakturerbar",
        _ => compensationType.ToString()
    };
}