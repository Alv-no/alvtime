using AlvTime.AlviterMigration.Models;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.Extensions.Logging;

namespace AlvTime.AlviterMigration.Services;

public class MigrationCalculator : IMigrationCalculator
{
    private readonly ILogger<MigrationCalculator> _logger;

    public MigrationCalculator(ILogger<MigrationCalculator> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<MigrationChange> CalculateMigrationChanges(
        IReadOnlyList<CsvTimeEntry> csvEntries,
        List<Hours> sourceHours,
        List<Hours> target336Hours)
    {
        var changes = new List<MigrationChange>();

        foreach (var group in csvEntries.GroupBy(e => (e.UserId, e.Date)))
        {
            var (userId, date) = group.Key;

            // Track per source entry: original DB value and total CSV amount to move.
            // Key = Hours.Id, Value = (dbValue, csvAmountToMove)
            var sourceUpdates = new Dictionary<int, (decimal DbValue, decimal CsvAmount)>();

            foreach (var entry in group)
            {
                if (entry.SourceTaskId == MigrationConstants.TargetTaskId)
                {
                    _logger.LogWarning(
                        "CSV entry for UserId={UserId}, Date={Date:yyyy-MM-dd} has sourceTaskId=336 (already the target task) — skipping",
                        entry.UserId, entry.Date);
                    continue;
                }

                var sourceEntry = sourceHours.FirstOrDefault(h =>
                    h.User == entry.UserId
                    && h.Date == entry.Date
                    && h.TaskId == entry.SourceTaskId);

                if (sourceEntry is null)
                {
                    _logger.LogWarning(
                        "No DB entry found for UserId={UserId}, Date={Date:yyyy-MM-dd}, TaskId={TaskId} — skipping",
                        entry.UserId, entry.Date, entry.SourceTaskId);
                    continue;
                }

                if (!sourceUpdates.ContainsKey(sourceEntry.Id))
                    sourceUpdates[sourceEntry.Id] = (sourceEntry.Value, 0);

                var (dbValue, csvAmount) = sourceUpdates[sourceEntry.Id];
                sourceUpdates[sourceEntry.Id] = (dbValue, csvAmount + entry.Value);
            }

            if (sourceUpdates.Count == 0)
                continue;

            decimal totalToMove = 0;
            var hourUpdates = new List<SourceHourUpdate>();

            foreach (var (id, (dbValue, csvAmount)) in sourceUpdates)
            {
                if (csvAmount > dbValue)
                {
                    throw new InvalidOperationException(
                        $"CSV wants to move {csvAmount}h from Hours.Id={id} " +
                        $"(UserId={userId}, Date={date:yyyy-MM-dd}) but DB only has {dbValue}h. Aborting.");
                }

                totalToMove += csvAmount;
                hourUpdates.Add(new SourceHourUpdate(id, dbValue - csvAmount));
            }

            if (totalToMove == 0)
                continue;

            var existing336 = target336Hours.FirstOrDefault(h =>
                h.User == userId && h.Date == date);

            changes.Add(new MigrationChange(
                UserId: userId,
                Date: date,
                NewTask336Value: (existing336?.Value ?? 0) + totalToMove,
                ExistingTask336Id: existing336?.Id,
                SourceHourUpdates: hourUpdates));
        }

        _logger.LogInformation(
            "Calculated {Changes} changes, {Updates} source entries to update",
            changes.Count,
            changes.Sum(c => c.SourceHourUpdates.Count));

        return changes;
    }
}
