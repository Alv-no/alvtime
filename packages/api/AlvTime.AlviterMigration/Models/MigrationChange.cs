namespace AlvTime.AlviterMigration.Models;

/// <summary>
/// Describes one database change for a specific user+date combination:
/// create or update the task-336 entry, and delete the source task entries.
/// </summary>
public record MigrationChange(
    int UserId,
    DateTime Date,
    decimal NewTask336Value,
    int? ExistingTask336Id,
    IReadOnlyList<int> SourceHourIdsToDelete
);
