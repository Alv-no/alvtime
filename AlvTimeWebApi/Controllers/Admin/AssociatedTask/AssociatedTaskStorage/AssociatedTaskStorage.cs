using AlvTime.Business.AssociatedTask;
using AlvTimeWebApi.Persistence.DatabaseModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTimeWebApi.Controllers.Admin.AssociatedTask.AssociatedTaskStorage
{
    public class AssociatedTaskStorage : IAssociatedTaskStorage
    {
        private readonly AlvTime_dbContext _context;

        public AssociatedTaskStorage(AlvTime_dbContext context)
        {
            _context = context;
        }
        public IEnumerable<AssociatedTaskResponseDto> GetAssociatedTasks()
        {
            return _context.AssociatedTasks
                .Select(at => new AssociatedTaskResponseDto
                {
                    Id = at.Id,
                    UserId = at.UserId,
                    TaskId = at.TaskId,
                    FromDate = at.FromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    EndDate = at.EndDate.Year == 1900 ? "N/A" : at.EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                }).ToList();
        }

        public AssociatedTaskResponseDto CreateAssociatedTask(AssociatedTaskRequestDto associatedTask)
        {
            var newAssociatedTask = new AssociatedTasks
            {
                UserId = associatedTask.UserId,
                TaskId = associatedTask.TaskId,
                FromDate = associatedTask.FromDate
            };

            _context.AssociatedTasks.Add(newAssociatedTask);
            _context.SaveChanges();

            var createdAssociatedTask = _context.AssociatedTasks
                .FirstOrDefault(at => at.TaskId == associatedTask.TaskId && at.UserId == associatedTask.UserId);

            return new AssociatedTaskResponseDto
            {
                Id = createdAssociatedTask.Id,
                UserId = createdAssociatedTask.UserId,
                TaskId = createdAssociatedTask.TaskId,
                FromDate = createdAssociatedTask.FromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                EndDate = createdAssociatedTask.EndDate.Year == 1900 ? "N/A" : createdAssociatedTask.EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
        }

        public AssociatedTaskResponseDto UpdateAssociatedTask(AssociatedTaskUpdateDto associatedTask)
        {
            var associatedTaskToBeUpdated = _context.AssociatedTasks
                .FirstOrDefault(at => at.Id == associatedTask.Id);

            if (associatedTask.UserId != null)
            {
                associatedTaskToBeUpdated.UserId = (int)associatedTask.UserId;
            }
            if (associatedTask.TaskId != null)
            {
                associatedTaskToBeUpdated.TaskId = (int)associatedTask.TaskId;
            }
            if (associatedTask.FromDate != null)
            {
                associatedTaskToBeUpdated.FromDate = (DateTime)associatedTask.FromDate;
            }
            if (associatedTask.EndDate != null)
            {
                associatedTaskToBeUpdated.EndDate = (DateTime)associatedTask.EndDate;
            }

            _context.SaveChanges();

            var updatedAssociatedTask = _context.AssociatedTasks
                .FirstOrDefault(at => at.Id == associatedTask.Id);

            return new AssociatedTaskResponseDto
            {
                Id = updatedAssociatedTask.Id,
                UserId = updatedAssociatedTask.UserId,
                TaskId = updatedAssociatedTask.TaskId,
                FromDate = updatedAssociatedTask.FromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                EndDate = updatedAssociatedTask.EndDate.Year == 1900 ? "N/A" : updatedAssociatedTask.EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)

            };
        }
    }
}
