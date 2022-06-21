using System.Linq;
using AlvTime.Business.Options;
using AlvTime.Business.Tasks;
using Microsoft.Extensions.Options;

namespace AlvTime.Business.Utils;

public class TaskUtils
{
    private readonly ITaskStorage _taskStorage;
    private readonly int _absenceProjectId;

    public TaskUtils(ITaskStorage taskStorage, IOptionsMonitor<TimeEntryOptions> timeEntryOptions)
    {
        _taskStorage = taskStorage;
        _absenceProjectId = timeEntryOptions.CurrentValue.AbsenceProject;
    }
        
    public bool TaskGivesOvertime(int taskId)
    {
        var task = _taskStorage.GetTasks(new TaskQuerySearch{ Id = taskId }).FirstOrDefault();
        return task != null && task.Project.Id != _absenceProjectId;
    }

    public bool TaskIsImposed(int taskId)
    {
        var task = _taskStorage.GetTasks(new TaskQuerySearch{ Id = taskId }).FirstOrDefault();
        return task != null && task.Imposed;
    }
}