namespace AlvTime.AlviterMigration.Models;

/// <summary>
/// Represents a single row read from the Alviter migration CSV file.
/// </summary>
public record CsvTimeEntry(int UserId, DateTime Date, int SourceTaskId, decimal Value);
