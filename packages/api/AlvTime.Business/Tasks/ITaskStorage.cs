using AlvTime.Business.Tasks.Admin;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlvTime.Business.Tasks
{
    public interface ITaskStorage
    {
        Task<IEnumerable<TaskResponseDto>> GetTasks(TaskQuerySearch criterias);
        Task<IEnumerable<TaskResponseDto>> GetUsersTasks(TaskQuerySearch criterias, int userId);
        Task CreateFavoriteTask(int taskId, int userId);
        Task RemoveFavoriteTask(int taskId, int userId);
        Task<bool> IsFavorite(int taskId, int userId);
        Task CreateTask(CreateTaskDto task);
        Task UpdateTask(UpdateTaskDto taskToBeUpdated);
    }

    public class TaskQuerySearch
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public int? Project { get; set; }
        public bool? Locked { get; set; }
    }
}
