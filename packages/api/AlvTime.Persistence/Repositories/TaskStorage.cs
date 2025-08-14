using AlvTime.Business.Customers;
using AlvTime.Business.Projects;
using AlvTime.Business.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = AlvTime.Persistence.DatabaseModels.Task;

namespace AlvTime.Persistence.Repositories
{
    public class TaskStorage : ITaskStorage
    {
        private readonly AlvTime_dbContext _context;

        public TaskStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetTasks(TaskQuerySearch criterias)
        {
            var tasks = await _context.Task
                .Include(t => t.CompensationRate).AsQueryable()
                .Filter(criterias)
                .Select(x => new TaskResponseDto
                {
                    Description = x.Description,
                    Id = x.Id,
                    Name = x.Name,
                    Locked = x.Locked,
                    Favorite = false,
                    CompensationRate = EnsureCompensationRate(x.CompensationRate),
                    Imposed = x.Imposed,
                    Project = new ProjectResponseDto
                    {
                        Id = x.ProjectNavigation.Id,
                        Name = x.ProjectNavigation.Name,
                        Customer = new CustomerDto
                        {
                            Id = x.ProjectNavigation.CustomerNavigation.Id,
                            Name = x.ProjectNavigation.CustomerNavigation.Name,
                            ContactEmail = x.ProjectNavigation.CustomerNavigation.ContactEmail,
                            ContactPerson = x.ProjectNavigation.CustomerNavigation.ContactPerson,
                            ContactPhone = x.ProjectNavigation.CustomerNavigation.ContactPhone,
                            InvoiceAddress = x.ProjectNavigation.CustomerNavigation.InvoiceAddress
                        }
                    }
                }).ToListAsync();

            return tasks;
        }

        private static decimal EnsureCompensationRate(IEnumerable<CompensationRate> compensationRate)
        {
            return compensationRate.MaxBy(cr => cr.FromDate)?.Value ?? 0.0M;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetUsersTasks(TaskQuerySearch criterias, int userId)
        {
            var usersFavoriteTasks = await _context.TaskFavorites.Where(x => x.UserId == userId).ToListAsync();

            var tasks = await GetTasks(criterias);

            foreach (var task in tasks)
            {
                task.Favorite = usersFavoriteTasks.Select(t => t.Id).Contains(task.Id);
                task.EnableComments = usersFavoriteTasks.Select(t => t.Id).Contains(task.Id) && usersFavoriteTasks.First(t => t.Id == task.Id).EnableComments;
            }

            return tasks;
        }

        public async System.Threading.Tasks.Task CreateTask(TaskDto task, int projectId)
        {
            var newTask = new Task
            {
                Description = task.Description,
                Favorite = false,
                Locked = task.Locked,
                Imposed = task.Imposed,
                Name = task.Name,
                Project = projectId,
                CompensationRate = new List<CompensationRate>
                {
                    new()
                    {
                        FromDate = new DateTime(1990, 1, 1),
                        Value = task.CompensationRate,
                    }
                }
            };
            _context.Task.Add(newTask);
            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task UpdateTask(TaskDto task)
        {
            var existingTask = await _context.Task
                .FirstOrDefaultAsync(x => x.Id == task.Id);

            existingTask.Locked = task.Locked;
            existingTask.Name = task.Name;
            existingTask.Description = task.Description ?? existingTask.Description;
            existingTask.Imposed = task.Imposed;

            var compensationRates =
                (await _context.CompensationRate.ToListAsync()).OrderByDescending(cr => cr.FromDate);
            var compRateToBeUpdated = compensationRates.First(cr => cr.TaskId == task.Id);
            compRateToBeUpdated.Value = task.CompensationRate;

            await _context.SaveChangesAsync();
        }

        public async System.Threading.Tasks.Task CreateFavoriteTask(int taskId, int userId)
        {
            TaskFavorites favorite = new TaskFavorites
            {
                TaskId = taskId,
                UserId = userId
            };
            _context.TaskFavorites.Add(favorite);
            await _context.SaveChangesAsync();
        }
        
        public async System.Threading.Tasks.Task ToggleCommentsOnFavoriteTask(int taskId, bool enableComments, int userId)
        {
            var favoriteEntry = await _context.TaskFavorites
                .FirstOrDefaultAsync(tf => tf.UserId == userId && tf.TaskId == taskId);

            if (favoriteEntry != null)
            {
                favoriteEntry.EnableComments = enableComments;
                await _context.SaveChangesAsync();
            }
        }

        public async System.Threading.Tasks.Task RemoveFavoriteTask(int taskId, int userId)
        {
            var favoriteEntry = await _context.TaskFavorites
                .FirstOrDefaultAsync(tf => tf.UserId == userId && tf.TaskId == taskId);

            _context.TaskFavorites.Remove(favoriteEntry);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsFavorite(int taskId, int userId)
        {
            return await _context.TaskFavorites
                .FirstOrDefaultAsync(tf => tf.UserId == userId && tf.TaskId == taskId) != null;
        }
    }
}