using AlvTime.AlviterMigration.Models;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.AlviterMigration.Services;

public class DatabaseWriter : IDatabaseWriter
{
    private readonly AlvTime_dbContext _context;
    private readonly ILogger<DatabaseWriter> _logger;

    public DatabaseWriter(AlvTime_dbContext context, ILogger<DatabaseWriter> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task ApplyAsync(IReadOnlyList<MigrationChange> changes)
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            foreach (var change in changes)
            {
                await UpsertTask336Async(change);
                await DeleteSourceEntriesAsync(change);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Applied {Count} changes successfully",
                changes.Count);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task UpsertTask336Async(MigrationChange change)
    {
        if (change.ExistingTask336Id.HasValue)
        {
            var existing = await _context.Hours.FindAsync(change.ExistingTask336Id.Value)
                ?? throw new InvalidOperationException($"Expected Hours entry with Id={change.ExistingTask336Id} not found.");

            existing.Value = change.NewTask336Value;
            existing.TimeRegistered = DateTime.UtcNow;

            _logger.LogDebug(
                "Updating task-336 entry Id={Id} for UserId={UserId}, Date={Date:yyyy-MM-dd} → {Value}h",
                existing.Id, change.UserId, change.Date, change.NewTask336Value);
        }
        else
        {
            var newEntry = new Hours
            {
                User = change.UserId,
                TaskId = 336,
                Date = change.Date,
                Value = change.NewTask336Value,
                Year = (short)change.Date.Year,
                DayNumber = (short)change.Date.DayOfYear,
                TimeRegistered = DateTime.UtcNow,
                Locked = false
            };

            _context.Hours.Add(newEntry);

            _logger.LogDebug(
                "Creating task-336 entry for UserId={UserId}, Date={Date:yyyy-MM-dd}, Value={Value}h",
                change.UserId, change.Date, change.NewTask336Value);
        }
    }

    private async Task DeleteSourceEntriesAsync(MigrationChange change)
    {
        foreach (var id in change.SourceHourIdsToDelete)
        {
            var entry = await _context.Hours.FindAsync(id)
                ?? throw new InvalidOperationException($"Expected source Hours entry with Id={id} not found.");

            _context.Hours.Remove(entry);

            _logger.LogDebug(
                "Deleting source entry Id={Id} (TaskId={TaskId}, UserId={UserId}, Date={Date:yyyy-MM-dd})",
                id, entry.TaskId, entry.User, entry.Date);
        }
    }
}
