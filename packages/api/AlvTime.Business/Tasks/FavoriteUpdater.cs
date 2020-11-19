using System.Linq;

namespace AlvTime.Business.Tasks
{
    public class FavoriteUpdater
    {
        private readonly ITaskStorage _taskStorage;

        public FavoriteUpdater(ITaskStorage taskStorage)
        {
            _taskStorage = taskStorage;
        }

        public TaskResponseDto UpdateFavoriteTasks(UpdateTasksDto taskToBeUpdated, int userId)
        {
            var userHasFavorite = _taskStorage.GetFavorite(taskToBeUpdated, userId);

            if (userHasFavorite && taskToBeUpdated.Favorite == false)
            {
                _taskStorage.RemoveFavoriteTask(taskToBeUpdated.Id, userId);
            }
            else if (!userHasFavorite && taskToBeUpdated.Favorite == true)
            {
                _taskStorage.CreateFavoriteTask(taskToBeUpdated.Id, userId);
            }

            return GetTask(taskToBeUpdated.Id, userId);
        }

        private TaskResponseDto GetTask(int taskId, int userId)
        {
            var taskResponseDto = _taskStorage.GetUsersTasks(new TaskQuerySearch
            {
                Id = taskId
            }, userId);

            return taskResponseDto.Single();
        }
    }
}
