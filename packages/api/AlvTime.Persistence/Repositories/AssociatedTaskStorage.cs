using AlvTime.Business.AssociatedTask;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;

namespace AlvTime.Persistence.Repositories
{
    public class AssociatedTaskStorage : IAssociatedTaskStorage
    {
        private readonly AlvTime_dbContext _context;

        public AssociatedTaskStorage(AlvTime_dbContext context)
        {
            _context = context;
        }
        
        public async Task<IEnumerable<AssociatedTaskResponseDto>> GetAssociatedTasks()
        {
            return await _context.AssociatedTasks
                .Select(at => new AssociatedTaskResponseDto
                {
                    Id = at.Id,
                    UserId = at.UserId,
                    TaskId = at.TaskId,
                    FromDate = at.FromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    EndDate = at.EndDate.Year == 1900 ? "N/A" : at.EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                }).ToListAsync();
        }

        public async Task<AssociatedTaskResponseDto> CreateAssociatedTask(AssociatedTaskRequestDto associatedTask)
        {
            var newAssociatedTask = new AssociatedTasks
            {
                UserId = associatedTask.UserId,
                TaskId = associatedTask.TaskId,
                FromDate = associatedTask.FromDate
            };

            _context.AssociatedTasks.Add(newAssociatedTask);
            await _context.SaveChangesAsync();

            var createdAssociatedTask = await _context.AssociatedTasks
                .FirstOrDefaultAsync(at => at.TaskId == associatedTask.TaskId && at.UserId == associatedTask.UserId);

            return new AssociatedTaskResponseDto
            {
                Id = createdAssociatedTask.Id,
                UserId = createdAssociatedTask.UserId,
                TaskId = createdAssociatedTask.TaskId,
                FromDate = createdAssociatedTask.FromDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                EndDate = createdAssociatedTask.EndDate.Year == 1900 ? "N/A" : createdAssociatedTask.EndDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
        }

        public async Task<AssociatedTaskResponseDto> UpdateAssociatedTask(AssociatedTaskUpdateDto associatedTask)
        {
            var associatedTaskToBeUpdated = await _context.AssociatedTasks
                .FirstOrDefaultAsync(at => at.Id == associatedTask.Id);

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

            await _context.SaveChangesAsync();

            var updatedAssociatedTask = await _context.AssociatedTasks
                .FirstOrDefaultAsync(at => at.Id == associatedTask.Id);

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
