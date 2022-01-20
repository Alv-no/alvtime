using AlvTime.Business.Tasks.Admin;
using System.Collections.Generic;

namespace AlvTime.Business.Tasks
{
    public interface ITaskStorage
    {
        IEnumerable<TaskResponseDto> GetTasks(TaskQuerySearch criterias);
        IEnumerable<TaskResponseDto> GetUsersTasks(TaskQuerySearch criterias, int userId);
        void CreateFavoriteTask(int taskId, int userId);
        void RemoveFavoriteTask(int taskId, int userId);
        bool GetFavorite(int taskId, int userId);
        void CreateTask(CreateTaskDto task);
        void UpdateTask(UpdateTasksDto taskToBeUpdated);
    }

    public class TaskQuerySearch
    {
        public int? Id { get; set; }
        public string Name { get; set; }

        public int? Project { get; set; }

        public bool? Locked { get; set; }
    }
}
