using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.AlviterMigration.Models;
using AlvTime.AlviterMigration.Services;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Tests.AlviterMigration;

public class MigrationCalculatorTests
{
    private static readonly DateTime Day1 = new DateTime(2025, 1, 2);
    private static readonly DateTime Day2 = new DateTime(2025, 1, 3);

    private MigrationCalculator CreateCalculator() =>
        new MigrationCalculator(NullLogger<MigrationCalculator>.Instance);

    [Fact]
    public void SimpleMoveCreatesNewTask336Entry()
    {
        var csvEntries = new List<CsvTimeEntry>
        {
            new(UserId: 33, Date: Day1, SourceTaskId: 59, Value: 2m)
        };
        var sourceHours = new List<Hours>
        {
            new() { Id = 1, User = 33, Date = Day1, TaskId = 59, Value = 2m, Locked = false }
        };

        var changes = CreateCalculator().Calculate(csvEntries, sourceHours, target336Hours: []);

        Assert.Single(changes);
        var change = changes[0];
        Assert.Equal(33, change.UserId);
        Assert.Equal(Day1, change.Date);
        Assert.Equal(2m, change.NewTask336Value);
        Assert.Null(change.ExistingTask336Id);
        Assert.Equal([1], change.SourceHourIdsToDelete);
    }

    [Fact]
    public void MoveSumsWithExistingTask336Entry()
    {
        var csvEntries = new List<CsvTimeEntry>
        {
            new(UserId: 33, Date: Day1, SourceTaskId: 59, Value: 2m)
        };
        var sourceHours = new List<Hours>
        {
            new() { Id = 1, User = 33, Date = Day1, TaskId = 59, Value = 2m, Locked = false }
        };
        var target336Hours = new List<Hours>
        {
            new() { Id = 99, User = 33, Date = Day1, TaskId = 336, Value = 2.5m }
        };

        var changes = CreateCalculator().Calculate(csvEntries, sourceHours, target336Hours);

        Assert.Single(changes);
        Assert.Equal(4.5m, changes[0].NewTask336Value);
        Assert.Equal(99, changes[0].ExistingTask336Id);
    }

    [Fact]
    public void MultipleSourceTasksSameDayAreGroupedIntoOneChange()
    {
        var csvEntries = new List<CsvTimeEntry>
        {
            new(UserId: 42, Date: Day1, SourceTaskId: 199, Value: 4m),
            new(UserId: 42, Date: Day1, SourceTaskId: 12, Value: 3.5m)
        };
        var sourceHours = new List<Hours>
        {
            new() { Id = 10, User = 42, Date = Day1, TaskId = 199, Value = 4m, Locked = false },
            new() { Id = 11, User = 42, Date = Day1, TaskId = 12, Value = 3.5m, Locked = false }
        };

        var changes = CreateCalculator().Calculate(csvEntries, sourceHours, target336Hours: []);

        Assert.Single(changes);
        var change = changes[0];
        Assert.Equal(7.5m, change.NewTask336Value);
        Assert.Equal(2, change.SourceHourIdsToDelete.Count);
        Assert.Contains(10, change.SourceHourIdsToDelete);
        Assert.Contains(11, change.SourceHourIdsToDelete);
    }

    [Fact]
    public void LockedSourceEntryIsIncluded()
    {
        var csvEntries = new List<CsvTimeEntry>
        {
            new(UserId: 33, Date: Day1, SourceTaskId: 59, Value: 2m)
        };
        var sourceHours = new List<Hours>
        {
            new() { Id = 1, User = 33, Date = Day1, TaskId = 59, Value = 2m, Locked = true }
        };

        var changes = CreateCalculator().Calculate(csvEntries, sourceHours, target336Hours: []);

        Assert.Single(changes);
        Assert.Equal(2m, changes[0].NewTask336Value);
        Assert.Equal([1], changes[0].SourceHourIdsToDelete);
    }

    [Fact]
    public void MissingSourceEntryInDbIsSkipped()
    {
        var csvEntries = new List<CsvTimeEntry>
        {
            new(UserId: 33, Date: Day1, SourceTaskId: 59, Value: 2m)
        };

        var changes = CreateCalculator().Calculate(csvEntries, sourceHours: [], target336Hours: []);

        Assert.Empty(changes);
    }

    [Fact]
    public void MultipleDaysAndUsersProduceIndependentChanges()
    {
        var csvEntries = new List<CsvTimeEntry>
        {
            new(UserId: 33, Date: Day1, SourceTaskId: 59, Value: 2m),
            new(UserId: 33, Date: Day2, SourceTaskId: 59, Value: 3m),
            new(UserId: 42, Date: Day1, SourceTaskId: 199, Value: 4m),
            new(UserId: 42, Date: Day2, SourceTaskId: 199, Value: 5m)
        };
        var sourceHours = new List<Hours>
        {
            new() { Id = 1, User = 33, Date = Day1, TaskId = 59, Value = 2m, Locked = false },
            new() { Id = 2, User = 33, Date = Day2, TaskId = 59, Value = 3m, Locked = false },
            new() { Id = 3, User = 42, Date = Day1, TaskId = 199, Value = 4m, Locked = false },
            new() { Id = 4, User = 42, Date = Day2, TaskId = 199, Value = 5m, Locked = false }
        };

        var changes = CreateCalculator().Calculate(csvEntries, sourceHours, target336Hours: []);

        Assert.Equal(4, changes.Count);
        Assert.Equal(2m, changes.Single(c => c.UserId == 33 && c.Date == Day1).NewTask336Value);
        Assert.Equal(3m, changes.Single(c => c.UserId == 33 && c.Date == Day2).NewTask336Value);
        Assert.Equal(4m, changes.Single(c => c.UserId == 42 && c.Date == Day1).NewTask336Value);
        Assert.Equal(5m, changes.Single(c => c.UserId == 42 && c.Date == Day2).NewTask336Value);
    }

    [Fact]
    public void UsesDbValueNotCsvValueWhenTheyDiffer()
    {
        // CSV was exported when DB had 2h, but DB was later updated to 7.5h.
        // We must move the full DB value to avoid losing hours.
        var csvEntries = new List<CsvTimeEntry>
        {
            new(UserId: 33, Date: Day1, SourceTaskId: 59, Value: 2m)
        };
        var sourceHours = new List<Hours>
        {
            new() { Id = 1, User = 33, Date = Day1, TaskId = 59, Value = 7.5m, Locked = false }
        };

        var changes = CreateCalculator().Calculate(csvEntries, sourceHours, target336Hours: []);

        Assert.Single(changes);
        Assert.Equal(7.5m, changes[0].NewTask336Value);
        Assert.Equal([1], changes[0].SourceHourIdsToDelete);
    }

    [Fact]
    public void DuplicateCsvRowsForSameDbEntryAreDeduped()
    {
        // Two CSV rows pointing to the same (userId, date, taskId) DB entry.
        // That DB entry should only be counted and deleted once.
        var csvEntries = new List<CsvTimeEntry>
        {
            new(UserId: 33, Date: Day1, SourceTaskId: 59, Value: 2m),
            new(UserId: 33, Date: Day1, SourceTaskId: 59, Value: 5.5m)
        };
        var sourceHours = new List<Hours>
        {
            new() { Id = 1, User = 33, Date = Day1, TaskId = 59, Value = 7.5m, Locked = false }
        };

        var changes = CreateCalculator().Calculate(csvEntries, sourceHours, target336Hours: []);

        Assert.Single(changes);
        Assert.Equal(7.5m, changes[0].NewTask336Value);
        Assert.Equal(1, changes[0].SourceHourIdsToDelete.Count);
        Assert.Equal([1], changes[0].SourceHourIdsToDelete);
    }

    [Fact]
    public void AllHoursInGroupAreMovedRegardlessOfLockStatus()
    {
        var csvEntries = new List<CsvTimeEntry>
        {
            new(UserId: 33, Date: Day1, SourceTaskId: 59, Value: 2m),
            new(UserId: 33, Date: Day1, SourceTaskId: 12, Value: 1.5m)
        };
        var sourceHours = new List<Hours>
        {
            new() { Id = 1, User = 33, Date = Day1, TaskId = 59, Value = 2m, Locked = false },
            new() { Id = 2, User = 33, Date = Day1, TaskId = 12, Value = 1.5m, Locked = true }
        };

        var changes = CreateCalculator().Calculate(csvEntries, sourceHours, target336Hours: []);

        Assert.Single(changes);
        Assert.Equal(3.5m, changes[0].NewTask336Value);
        Assert.Contains(1, changes[0].SourceHourIdsToDelete);
        Assert.Contains(2, changes[0].SourceHourIdsToDelete);
    }
}
