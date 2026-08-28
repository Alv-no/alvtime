using AlvTime.AlviterMigration.Models;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.AlviterMigration.Services;

public interface IMigrationCalculator
{
    IReadOnlyList<MigrationChange> CalculateMigrationChanges(
        IReadOnlyList<CsvTimeEntry> csvEntries,
        List<Hours> sourceHours,
        List<Hours> target336Hours);
}
