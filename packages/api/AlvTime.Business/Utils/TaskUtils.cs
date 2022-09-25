using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        
    public async Task<bool> TaskGivesOvertime(int taskId)
    {
        var task = (await _taskStorage.GetTasks(new TaskQuerySearch{ Id = taskId })).FirstOrDefault();
        return task != null && task.Project.Id != _absenceProjectId;
    }

    public async Task<bool> TaskIsImposed(int taskId)
    {
        var task = (await _taskStorage.GetTasks(new TaskQuerySearch{ Id = taskId })).FirstOrDefault();
        return task != null && task.Imposed;
    }
    
    public async Task<List<int>> GetAllImposedTaskIds()
    {
        var allTasks = await _taskStorage.GetTasks(new TaskQuerySearch());
        var imposedTasks = allTasks.Where(t => t.Imposed);
        return imposedTasks.Select(t => t.Id).ToList();
    }
}