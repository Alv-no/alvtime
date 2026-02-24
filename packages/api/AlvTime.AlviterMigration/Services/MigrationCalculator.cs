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

    public IReadOnlyList<MigrationChange> Calculate(
        IReadOnlyList<CsvTimeEntry> csvEntries,
        List<Hours> sourceHours,
        List<Hours> target336Hours)
    {
        var changes = new List<MigrationChange>();

        foreach (var group in csvEntries.GroupBy(e => (e.UserId, e.Date)))
        {
            var (userId, date) = group.Key;

            decimal totalToMove = 0;
            var sourceIdsToDelete = new List<int>();

            foreach (var entry in group)
            {
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

                if (sourceEntry.Locked)
                {
                    _logger.LogWarning(
                        "Entry is locked: UserId={UserId}, Date={Date:yyyy-MM-dd}, TaskId={TaskId} — skipping",
                        entry.UserId, entry.Date, entry.SourceTaskId);
                    continue;
                }

                totalToMove += entry.Value;
                sourceIdsToDelete.Add(sourceEntry.Id);
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
                SourceHourIdsToDelete: sourceIdsToDelete));
        }

        _logger.LogInformation(
            "Calculated {Changes} changes, {Deletes} source entries to delete",
            changes.Count,
            changes.Sum(c => c.SourceHourIdsToDelete.Count));

        return changes;
    }
}
