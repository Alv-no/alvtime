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

        public TaskResponseDto CreateTask(CreateTaskDto task)
        {
            if (!GetTask(task).Any())
            {
                _storage.CreateTask(task);
            }

            return GetTask(task).Single();
        }

        public TaskResponseDto UpdateTask(UpdateTasksDto taskToBeUpdated)
        {
            _storage.UpdateTask(taskToBeUpdated);

            return _storage.GetTasks(new TaskQuerySearch
            {
                Id = taskToBeUpdated.Id
            }).Single();
        }

        private IEnumerable<TaskResponseDto> GetTask(CreateTaskDto task)
        {
            return _storage.GetTasks(new TaskQuerySearch
            {
                Name = task.Name,
                Project = task.Project
            });
        }
    }
}
