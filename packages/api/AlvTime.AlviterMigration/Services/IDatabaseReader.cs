using AlvTime.AlviterMigration.Models;
using AlvTime.Persistence.DatabaseModels;

namespace AlvTime.AlviterMigration.Services;

public interface IDatabaseReader
{
    Task<(List<Hours> SourceHours, List<Hours> Target336Hours)> ReadAsync(IReadOnlyList<CsvTimeEntry> csvEntries);
}
