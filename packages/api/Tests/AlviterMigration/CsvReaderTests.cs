using System;
using System.IO;
using AlvTime.AlviterMigration.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Tests.AlviterMigration;

public class CsvReaderTests : IDisposable
{
    private readonly string _tempFile;
    private readonly CsvReader _reader;

    public CsvReaderTests()
    {
        _tempFile = Path.GetTempFileName() + ".csv";
        _reader = new CsvReader(NullLogger<CsvReader>.Instance);
    }

    public void Dispose() => File.Delete(_tempFile);

    private void WriteCsv(string content) => File.WriteAllText(_tempFile, content);

    [Fact]
    public void ReadsStandardRowsCorrectly()
    {
        WriteCsv(
            "taskID,taskName,projectID,value,comment,userID,date,userName,email\n" +
            "59,Infoskjerm,91,2,,33,02/01/2025 00:00,Some User,user@alv.no\n" +
            "199,Uvær utvikling,91,4,,42,03/01/2025 00:00,Other User,other@alv.no\n");

        var entries = _reader.Read(_tempFile);

        Assert.Equal(2, entries.Count);

        var first = entries[0];
        Assert.Equal(33, first.UserId);
        Assert.Equal(new DateTime(2025, 1, 2), first.Date);
        Assert.Equal(59, first.SourceTaskId);
        Assert.Equal(2m, first.Value);

        var second = entries[1];
        Assert.Equal(42, second.UserId);
        Assert.Equal(new DateTime(2025, 1, 3), second.Date);
        Assert.Equal(199, second.SourceTaskId);
        Assert.Equal(4m, second.Value);
    }

    [Fact]
    public void SkipsRowsWithUnparseableValue()
    {
        WriteCsv(
            "taskID,taskName,projectID,value,comment,userID,date\n" +
            "59,Test,91,invalid,,33,02/01/2025 00:00\n" +
            "199,Test,91,4,,42,03/01/2025 00:00\n");

        var entries = _reader.Read(_tempFile);

        Assert.Single(entries);
        Assert.Equal(42, entries[0].UserId);
    }

    [Fact]
    public void SkipsRowsWithUnparseableDate()
    {
        WriteCsv(
            "taskID,taskName,projectID,value,comment,userID,date\n" +
            "59,Test,91,2,,33,not-a-date\n" +
            "199,Test,91,4,,42,03/01/2025 00:00\n");

        var entries = _reader.Read(_tempFile);

        Assert.Single(entries);
    }

    [Fact]
    public void HandlesDecimalValueWithDotSeparator()
    {
        WriteCsv(
            "taskID,taskName,projectID,value,comment,userID,date\n" +
            "59,Test,91,3.5,,33,02/01/2025 00:00\n");

        var entries = _reader.Read(_tempFile);

        Assert.Single(entries);
        Assert.Equal(3.5m, entries[0].Value);
    }

    [Fact]
    public void ThrowsWhenRequiredColumnIsMissing()
    {
        WriteCsv("taskID,taskName,projectID\nsome,data,here\n");

        Assert.Throws<InvalidOperationException>(() => _reader.Read(_tempFile));
    }

    [Fact]
    public void ThrowsWhenFileDoesNotExist()
    {
        Assert.Throws<FileNotFoundException>(() => _reader.Read("/nonexistent/path/file.csv"));
    }

    [Fact]
    public void ColumnLookupIsCaseInsensitive()
    {
        WriteCsv(
            "TaskID,TaskName,ProjectID,Value,Comment,UserID,Date\n" +
            "59,Test,91,2,,33,02/01/2025 00:00\n");

        var entries = _reader.Read(_tempFile);

        Assert.Single(entries);
    }

    [Fact]
    public void HandlesQuotedFieldsWithCommas()
    {
        WriteCsv(
            "taskID,taskName,projectID,value,comment,userID,date,customerName\n" +
            "59,\"Task, with comma\",91,2,,33,02/01/2025 00:00,\"Customer, Inc\"\n");

        var entries = _reader.Read(_tempFile);

        Assert.Single(entries);
        Assert.Equal(59, entries[0].SourceTaskId);
        Assert.Equal(2m, entries[0].Value);
    }
}
