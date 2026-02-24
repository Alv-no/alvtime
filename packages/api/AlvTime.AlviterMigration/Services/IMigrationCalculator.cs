using AlvTime.AlviterMigration.Models;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.AlviterMigration.Services;

public interface IMigrationCalculator
{
    IReadOnlyList<MigrationChange> Calculate(
        IReadOnlyList<CsvTimeEntry> csvEntries,
        List<Hours> sourceHours,
        List<Hours> target336Hours);
}
