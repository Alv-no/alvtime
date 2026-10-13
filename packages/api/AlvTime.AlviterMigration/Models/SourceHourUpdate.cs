namespace AlvTime.AlviterMigration.Models;

/// <summary>
/// Describes how a source Hours entry should be updated after migration:
/// the value is reduced by the moved amount, never deleted.
/// </summary>
public record SourceHourUpdate(int Id, decimal NewValue);
