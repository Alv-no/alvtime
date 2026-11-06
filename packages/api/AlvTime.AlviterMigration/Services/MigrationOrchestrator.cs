using Microsoft.Extensions.Logging;

namespace AlvTime.AlviterMigration.Services;

public class MigrationOrchestrator : IMigrationOrchestrator
{
    private readonly ICsvReader _csvReader;
    private readonly IDatabaseReader _databaseReader;
    private readonly IMigrationCalculator _calculator;
    private readonly IDatabaseWriter _databaseWriter;
    private readonly ILogger<MigrationOrchestrator> _logger;

    public MigrationOrchestrator(
        ICsvReader csvReader,
        IDatabaseReader databaseReader,
        IMigrationCalculator calculator,
        IDatabaseWriter databaseWriter,
        ILogger<MigrationOrchestrator> logger)
    {
        _csvReader = csvReader;
        _databaseReader = databaseReader;
        _calculator = calculator;
        _databaseWriter = databaseWriter;
        _logger = logger;
    }

    public async Task RunAsync(string csvFilePath)
    {
        _logger.LogInformation("Starting Alviter migration from {File}", csvFilePath);

        var csvEntries = _csvReader.Read(csvFilePath);
        _logger.LogInformation("Step 1/4: Read {Count} entries from CSV", csvEntries.Count);

        var (sourceHours, target336Hours) = await _databaseReader.LoadSourceAndTarget336HoursAsync(csvEntries);
        _logger.LogInformation(
            "Step 2/4: Loaded {Source} source entries and {Target} existing task-336 entries from database",
            sourceHours.Count, target336Hours.Count);

        var changes = _calculator.CalculateMigrationChanges(csvEntries, sourceHours, target336Hours);
        _logger.LogInformation(
            "Step 3/4: Calculated {Changes} changes ({Updates} source entries to update)",
            changes.Count,
            changes.Sum(c => c.SourceHourUpdates.Count));

        await _databaseWriter.ExecuteMigrationAsync(changes);
        _logger.LogInformation("Step 4/4: All changes written to database. Migration complete.");
    }
}
