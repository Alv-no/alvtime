using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Options;
using AlvTime.Business.Tasks;
using Microsoft.Extensions.Options;

namespace AlvTime.Business.Utils;

public class TaskUtils(ITaskStorage taskStorage, IOptionsMonitor<TimeEntryOptions> timeEntryOptions)
{
    private readonly int _absenceProjectId = timeEntryOptions.CurrentValue.AbsenceProject;

    public bool ProjectIsAbsence(int projectId)
    {
        return projectId == _absenceProjectId;
    }
        
    public async Task<bool> TaskGivesOvertime(int taskId)
    {
        var task = (await taskStorage.GetTasks(new TaskQuerySearch{ Id = taskId })).FirstOrDefault();
        return task != null && task.Project.Id != _absenceProjectId;
    }

    public async Task<List<int>> GetAllImposedTaskIds()
    {
        var allTasks = await taskStorage.GetTasks(new TaskQuerySearch());
        var imposedTasks = allTasks.Where(t => t.Imposed);
        return imposedTasks.Select(t => t.Id).ToList();
    }

    public async Task<bool> TaskIsLocked(int taskId, DateTime desiredRegisterDate)
    {
        var task = (await taskStorage.GetTasks(new TaskQuerySearch { Id = taskId })).FirstOrDefault();
        return task != null && task.Locked || task != null && task.Project.Customer.LockedTo >= desiredRegisterDate;
    }
}