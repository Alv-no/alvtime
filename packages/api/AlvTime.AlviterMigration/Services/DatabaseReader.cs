using AlvTime.AlviterMigration.Models;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AlvTime.AlviterMigration.Services;

public class DatabaseReader : IDatabaseReader
{
    private readonly AlvTime_dbContext _context;
    private readonly ILogger<DatabaseReader> _logger;

    public DatabaseReader(AlvTime_dbContext context, ILogger<DatabaseReader> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<(List<Hours> SourceHours, List<Hours> Target336Hours)> ReadAsync(IReadOnlyList<CsvTimeEntry> csvEntries)
    {
        var userIds = csvEntries.Select(e => e.UserId).Distinct().ToList();
        var dates = csvEntries.Select(e => e.Date).Distinct().ToList();
        var sourceTaskIds = csvEntries.Select(e => e.SourceTaskId).Distinct().ToList();

        var sourceHours = await _context.Hours
            .Where(h => userIds.Contains(h.User)
                     && dates.Contains(h.Date)
                     && sourceTaskIds.Contains(h.TaskId))
            .ToListAsync();

        var target336Hours = await _context.Hours
            .Where(h => userIds.Contains(h.User)
                     && dates.Contains(h.Date)
                     && h.TaskId == 336)
            .ToListAsync();

        _logger.LogInformation(
            "Found {Source} source entries and {Target} existing task-336 entries in database",
            sourceHours.Count, target336Hours.Count);

        return (sourceHours, target336Hours);
    }
}
