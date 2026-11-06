namespace AlvTime.AlviterMigration.Models;

/// <summary>
/// Describes one database change for a specific user+date combination:
/// create or update the task-336 entry, and reduce the source task entries to their remainder.
/// Source entries are never deleted — they are set to 0 or the remaining hours.
/// </summary>
public record MigrationChange(
    int UserId,
    DateTime Date,
    decimal NewTask336Value,
    int? ExistingTask336Id,
    IReadOnlyList<SourceHourUpdate> SourceHourUpdates
);
