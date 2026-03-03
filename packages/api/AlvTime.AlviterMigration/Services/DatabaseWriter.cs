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
        var affectedUserIds = changes.Select(c => c.UserId).Distinct().ToList();

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var preTotals = await QueryHourTotalsPerUserAsync(affectedUserIds);

            foreach (var change in changes)
            {
                await UpsertTask336Async(change);
                await UpdateSourceEntriesAsync(change);
            }

            await _context.SaveChangesAsync();

            var postTotals = await QueryHourTotalsPerUserAsync(affectedUserIds);

            ValidateTotals(preTotals, postTotals);

            await transaction.CommitAsync();

            _logger.LogInformation(
                "Applied {Count} changes successfully. Total hours before: {Pre}, after: {Post}",
                changes.Count,
                preTotals.Values.Sum(),
                postTotals.Values.Sum());
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task<Dictionary<int, decimal>> QueryHourTotalsPerUserAsync(List<int> userIds)
    {
        return await _context.Hours
            .AsNoTracking()
            .Where(h => userIds.Contains(h.User))
            .GroupBy(h => h.User)
            .Select(g => new { UserId = g.Key, Total = g.Sum(h => h.Value) })
            .ToDictionaryAsync(x => x.UserId, x => x.Total);
    }

    private void ValidateTotals(Dictionary<int, decimal> pre, Dictionary<int, decimal> post)
    {
        var preGlobal = pre.Values.Sum();
        var postGlobal = post.Values.Sum();

        var allUserIds = pre.Keys.Union(post.Keys).ToList();
        var usersWithDiff = allUserIds
            .Select(uid => (UserId: uid, Pre: pre.GetValueOrDefault(uid, 0m), Post: post.GetValueOrDefault(uid, 0m)))
            .Where(x => x.Pre != x.Post)
            .ToList();

        foreach (var (userId, userPre, userPost) in usersWithDiff)
        {
            _logger.LogError(
                "Hour total mismatch for UserId={UserId}: {Pre}h → {Post}h (diff: {Diff}h)",
                userId, userPre, userPost, userPost - userPre);
        }

        if (preGlobal != postGlobal)
        {
            throw new InvalidOperationException(
                $"Validation failed: global hour total changed from {preGlobal} to {postGlobal} " +
                $"(diff: {postGlobal - preGlobal}). {usersWithDiff.Count} user(s) affected. Rolling back.");
        }

        _logger.LogInformation(
            "Validation passed: global total {Total}h unchanged across {Users} affected users",
            preGlobal, pre.Count);
    }

    private async Task UpsertTask336Async(MigrationChange change)
    {
        if (change.ExistingTask336Id.HasValue)
        {
            var existing = await _context.Hours.FindAsync(change.ExistingTask336Id.Value)
                ?? throw new InvalidOperationException($"Expected Hours entry with Id={change.ExistingTask336Id} not found.");

            var oldValue = existing.Value;
            existing.Value = change.NewTask336Value;
            existing.TimeRegistered = DateTime.UtcNow;

            _logger.LogDebug(
                "Updating task-336 entry Id={Id} for UserId={UserId}, Date={Date:yyyy-MM-dd} {OldValue}h → {NewValue}h",
                existing.Id, change.UserId, change.Date, oldValue, change.NewTask336Value);
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

    private async Task UpdateSourceEntriesAsync(MigrationChange change)
    {
        foreach (var update in change.SourceHourUpdates)
        {
            var entry = await _context.Hours.FindAsync(update.Id)
                ?? throw new InvalidOperationException($"Expected source Hours entry with Id={update.Id} not found.");

            _logger.LogDebug(
                "Updating source entry Id={Id} (TaskId={TaskId}, UserId={UserId}, Date={Date:yyyy-MM-dd}) {OldValue}h → {NewValue}h",
                entry.Id, entry.TaskId, entry.User, entry.Date, entry.Value, update.NewValue);

            entry.Value = update.NewValue;
            entry.TimeRegistered = DateTime.UtcNow;
        }
    }
}
