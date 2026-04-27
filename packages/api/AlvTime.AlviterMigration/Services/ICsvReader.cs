using AlvTime.AlviterMigration.Models;

namespace AlvTime.AlviterMigration.Services;

public interface ICsvReader
{
    IReadOnlyList<CsvTimeEntry> Read(string filePath);
}
