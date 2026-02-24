using AlvTime.AlviterMigration.Models;

namespace AlvTime.AlviterMigration.Services;

public interface IDatabaseWriter
{
    Task ApplyAsync(IReadOnlyList<MigrationChange> changes);
}
