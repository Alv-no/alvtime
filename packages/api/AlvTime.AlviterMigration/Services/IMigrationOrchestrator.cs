namespace AlvTime.AlviterMigration.Services;

public interface IMigrationOrchestrator
{
    Task RunAsync(string csvFilePath);
}
