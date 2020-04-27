using System.Collections.Generic;
using System.Linq;

namespace AlvTime.Business.Tasks.Admin
{
    public class TaskCreator
    {
        private readonly ITaskStorage _storage;

        public TaskCreator(ITaskStorage storage)
        {
            _storage = storage;
        }

        public TaskResponseDto CreateTask(CreateTaskDto task, int userId)
        {
            if (!GetTask(task, userId).Any())
            {
                _storage.CreateTask(task, userId);
            }

            return GetTask(task, userId).Single();
        }

        public TaskResponseDto UpdateTask(UpdateTasksDto taskToBeUpdated, int userId)
        {
            _storage.UpdateTask(taskToBeUpdated);

            return _storage.GetTasks(new TaskQuerySearch
            {
                Id = taskToBeUpdated.Id
            }, userId).Single();
        }

        private IEnumerable<TaskResponseDto> GetTask(CreateTaskDto task, int userId)
        {
            return _storage.GetTasks(new TaskQuerySearch
            {
                Name = task.Name,
                Project = task.Project
            }, userId);
        }
    }
}
