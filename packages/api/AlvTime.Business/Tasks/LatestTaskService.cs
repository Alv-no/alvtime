using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.TimeRegistration;

namespace AlvTime.Business.Tasks
{
    public class LatestTaskService
    {
        private readonly TaskService _taskService;
        private readonly TimeRegistrationService _timeRegistrationService;

        public LatestTaskService(TaskService taskService, TimeRegistrationService timeRegistrationService)
        {
            _taskService = taskService;
            _timeRegistrationService = timeRegistrationService;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetLatestTasksForUser()
        {
            var timeEntries = await _timeRegistrationService.GetTimeEntries(new TimeEntryQuerySearch
            {
                FromDateInclusive = DateTime.Now.AddDays(-30),
                ToDateInclusive = DateTime.Now
            });

            var mostRecentlyUsedTaskIds = timeEntries.OrderByDescending(item => item.Date).Select(dto => dto.TaskId)
                .Distinct().Take(5).ToList();
            var taskResponses = new List<TaskResponseDto>();
        
            foreach (var taskId in mostRecentlyUsedTaskIds)
            {
                var tasksForUser = await _taskService.GetTasksForUser(new TaskQuerySearch { Id = taskId });
                if (tasksForUser.IsSuccess)
                    taskResponses.AddRange(tasksForUser.Value);
            }

            return taskResponses;
        }
    }
}