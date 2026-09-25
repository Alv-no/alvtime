using AlvTime.AlviterMigration.Models;

namespace AlvTime.AlviterMigration.Services;

public interface IDatabaseWriter
{
    Task ExecuteMigrationAsync(IReadOnlyList<MigrationChange> changes);
}
