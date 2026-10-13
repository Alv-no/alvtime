using System.Globalization;
using AlvTime.AlviterMigration.Models;
using Microsoft.Extensions.Logging;
using Microsoft.VisualBasic.FileIO;

namespace AlvTime.AlviterMigration.Services;

public class CsvReader : ICsvReader
{
    private readonly ILogger<CsvReader> _logger;

    public CsvReader(ILogger<CsvReader> logger)
    {
        _logger = logger;
    }

    public IReadOnlyList<CsvTimeEntry> Read(string filePath)
    {
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"CSV file not found: {filePath}");

        using var parser = new TextFieldParser(filePath, System.Text.Encoding.UTF8);
        parser.TextFieldType = FieldType.Delimited;
        parser.SetDelimiters(",");
        parser.HasFieldsEnclosedInQuotes = true;

        var headers = parser.ReadFields()
            ?? throw new InvalidOperationException("CSV file is empty or missing header row.");

        var colTaskId = FindColumn(headers, "taskid");
        var colValue = FindColumn(headers, "value");
        var colUserId = FindColumn(headers, "userid");
        var colDate = FindColumn(headers, "date");

        var entries = new List<CsvTimeEntry>();
        int lineNumber = 1;

        while (!parser.EndOfData)
        {
            lineNumber++;
            var fields = parser.ReadFields();
            if (fields is null) continue;

            if (!TryParseRow(fields, lineNumber, colTaskId, colValue, colUserId, colDate, out var entry))
                continue;

            entries.Add(entry!);
        }

        _logger.LogInformation("Read {Count} entries from {File}", entries.Count, filePath);
        return entries;
    }

    private bool TryParseRow(
        string[] fields, int lineNumber,
        int colTaskId, int colValue, int colUserId, int colDate,
        out CsvTimeEntry? entry)
    {
        entry = null;

        if (!int.TryParse(fields[colTaskId], out var sourceTaskId))
        {
            _logger.LogWarning("Line {Line}: could not parse taskID '{Val}' — skipping", lineNumber, fields[colTaskId]);
            return false;
        }

        if (!decimal.TryParse(fields[colValue], NumberStyles.Number, CultureInfo.InvariantCulture, out var value))
        {
            _logger.LogWarning("Line {Line}: could not parse value '{Val}' — skipping", lineNumber, fields[colValue]);
            return false;
        }

        if (!int.TryParse(fields[colUserId], out var userId))
        {
            _logger.LogWarning("Line {Line}: could not parse userID '{Val}' — skipping", lineNumber, fields[colUserId]);
            return false;
        }

        if (!DateTime.TryParseExact(fields[colDate], "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
        {
            _logger.LogWarning("Line {Line}: could not parse date '{Val}' — skipping", lineNumber, fields[colDate]);
            return false;
        }

        entry = new CsvTimeEntry(userId, date.Date, sourceTaskId, value);
        return true;
    }

    private static int FindColumn(string[] headers, string name)
    {
        var index = Array.FindIndex(headers, h => h.Trim().Equals(name, StringComparison.OrdinalIgnoreCase));
        if (index < 0)
            throw new InvalidOperationException($"Required column '{name}' not found in CSV header.");
        return index;
    }
}
