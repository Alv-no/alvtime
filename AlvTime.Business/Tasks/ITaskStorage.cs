﻿using AlvTime.Business.Tasks.Admin;
using System.Collections.Generic;

namespace AlvTime.Business.Tasks
{
    public interface ITaskStorage
    {
        IEnumerable<TaskResponseDto> GetTasks(TaskQuerySearch criterias, int userId);
        void CreateFavoriteTask(int taskId, int userId);
        void RemoveFavoriteTask(int taskId, int userId);
        bool GetFavorite(UpdateTasksDto taskToBeUpdated, int userId);
        void CreateTask(CreateTaskDto task, int userId);
        void UpdateTask(UpdateTasksDto taskToBeUpdated);
    }

    public class TaskQuerySearch
    {
        public int? Id { get; set; }
        public string Name { get; set; }

        public int? Project { get; set; }

        public bool? Locked { get; set; }

        public decimal? CompensationRate { get; set; }
    }
}