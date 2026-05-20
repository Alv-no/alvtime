using AlvTime.Business.Customers;
using AlvTime.Business.Projects;
using AlvTime.Business.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.CompensationRate;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = AlvTime.Persistence.DatabaseModels.Task;
using User = AlvTime.Business.Users.User;

namespace AlvTime.Persistence.Repositories;

public class TaskStorage(AlvTime_dbContext context) : ITaskStorage
{
    public async Task<IEnumerable<TaskResponseDto>> GetTasks(TaskQuerySearch criterias)
    {
        var tasks = await context.Task
            .AsQueryable()
            .Filter(criterias)
            .Select(x => new TaskResponseDto
            {
                Description = x.Description,
                Id = x.Id,
                Name = x.Name,
                Locked = x.Locked,
                Favorite = false,
                Imposed = x.Imposed,
                CompensationType = x.CompensationType,
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

    public async Task<IEnumerable<TaskResponseDto>> GetUsersTasks(TaskQuerySearch criterias, User user)
    {
        var usersFavoriteTasks = await context.TaskFavorites.Where(x => x.UserId == user.Id).ToListAsync();

        var tasks = await GetTasks(criterias);

        var favoriteIds = usersFavoriteTasks.Select(t => t.TaskId).ToList();
        foreach (var task in tasks)
        {
            task.Favorite = favoriteIds.Contains(task.Id);
            task.EnableComments = usersFavoriteTasks.Select(t => t.Id).Contains(task.Id) && usersFavoriteTasks.First(t => t.Id == task.Id).EnableComments;
            task.CompensationRate =
                CompensationRateHelper.ResolveCompensationRate(task.CompensationType, task.Imposed, user.SalaryModel);
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
            CompensationType = task.CompensationType
        };
        context.Task.Add(newTask);
        await context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task UpdateTask(TaskDto task)
    {
        var existingTask = await context.Task
            .FirstOrDefaultAsync(x => x.Id == task.Id);

        existingTask.Locked = task.Locked;
        existingTask.Name = task.Name;
        existingTask.Description = task.Description ?? existingTask.Description;
        existingTask.Imposed = task.Imposed;

        await context.SaveChangesAsync();
    }

    public async System.Threading.Tasks.Task CreateFavoriteTask(int taskId, int userId)
    {
        TaskFavorites favorite = new TaskFavorites
        {
            TaskId = taskId,
            UserId = userId
        };
        context.TaskFavorites.Add(favorite);
        await context.SaveChangesAsync();
    }
        
    public async System.Threading.Tasks.Task ToggleCommentsOnFavoriteTask(int taskId, bool enableComments, int userId)
    {
        var favoriteEntry = await context.TaskFavorites
            .FirstOrDefaultAsync(tf => tf.UserId == userId && tf.TaskId == taskId);

        if (favoriteEntry != null)
        {
            favoriteEntry.EnableComments = enableComments;
            await context.SaveChangesAsync();
        }
    }

    public async System.Threading.Tasks.Task RemoveFavoriteTask(int taskId, int userId)
    {
        var favoriteEntry = await context.TaskFavorites
            .FirstOrDefaultAsync(tf => tf.UserId == userId && tf.TaskId == taskId);

        context.TaskFavorites.Remove(favoriteEntry);
        await context.SaveChangesAsync();
    }

    public async Task<bool> IsFavorite(int taskId, int userId)
    {
        return await context.TaskFavorites
            .FirstOrDefaultAsync(tf => tf.UserId == userId && tf.TaskId == taskId) != null;
    }

    public async Task<TaskResponseDto> GetTaskById(int taskId)
    {
        var task = await context.Task.FirstOrDefaultAsync(t => t.Id == taskId);
        return new TaskResponseDto
        {
            Id = task.Id,
            Name = task.Name,
            Description = task.Description,
            Favorite = task.Favorite,
            Locked = task.Locked,
            Imposed = task.Imposed,
            CompensationType = task.CompensationType
        };
    }
}