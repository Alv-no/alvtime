using AlvTime.Business.FlexiHours;
using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Tests.UnitTests.Flexihours
{
    public class GetFlexihoursTests
    {
        [Fact]
        public void GetFlexhours_NoWorkAtAll_Minus1WorkDayInFlexhour()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var calculator = new FlexhourCalculator(context);
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Contains(flexhours, hour => hour.Value == -7.5M);
        }

        [Fact]
        public void GetFlexhours_NoWorkFor2Days_Minus2WorkDaysInFlexhour()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var calculator = new FlexhourCalculator(context);
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 02), 1);

            Assert.True(flexhours.Sum(item => item.Value) == -15.0M);
        }

        [Fact]
        public void GetFlexhours_NoWorkFor2DaysAndRecorded0ForOneOfThem_Minus2WorkDaysInFlexhour()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            var calculator = new FlexhourCalculator(context);

            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 01, 01),
                Value = 0
            });

            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 02), 1);

            Assert.True(flexhours.Sum(item => item.Value) == -15.0M);
        }

        [Fact]
        public void GetFlexhours_NoOvertime_NoFlexhour()
        {
            var context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();

            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 01, 01),
                Value = 7.5M
            });

            context.SaveChanges();

            var calculator = new FlexhourCalculator(context);
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Empty(flexhours);
        }

        [Fact]
        public void GetFlexhours_Worked10Hours_2AndAHalfHourInFlex()
        {
            var context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();

            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 01, 01),
                Value = 10M
            });

            context.SaveChanges();

            var calculator = new FlexhourCalculator(context);
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Contains(flexhours, hour => hour.Value == 2.5M);
        }

        [Fact]
        public void GetFlexhours_Worked5Hours_Minus2AndAHalfHourInFlex()
        {
            var context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();

            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 01, 01),
                Value = 5M
            });

            context.SaveChanges();

            var calculator = new FlexhourCalculator(context);
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Contains(flexhours, hour => hour.Value == -2.5M);
        }

        [Fact]
        public void GetFlexhours_Worked10HoursAnd10Hours_5HourInFlex()
        {
            var context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();

            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 01, 01),
                Value = 10M
            });
            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 01, 02),
                Value = 10M
            });

            context.SaveChanges();

            var calculator = new FlexhourCalculator(context);
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 02), 1);

            Assert.Contains(flexhours, hour => hour.Value == 2.5M && hour.Date == new DateTime(2020, 01, 01));
            Assert.Contains(flexhours, hour => hour.Value == 2.5M && hour.Date == new DateTime(2020, 01, 02));
        }

        [Fact]
        public void GetFlexhours_Worked0HoursAnd7AndAHalfHours_NoHourInFlex()
        {
            var context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();

            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 01, 01),
                Value = 0M
            });
            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 01, 01),
                Value = 7.5M
            });

            context.SaveChanges();

            var calculator = new FlexhourCalculator(context);
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Empty(flexhours);
        }

        [Fact]
        public void GetFlexhours_YouWorked10HoursAndSomeoneElseWorked8Hours_2AndAHalfHoursInFlex()
        {
            var context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();

            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 01, 01),
                Value = 10M
            });

            context.Hours.Add(new Hours
            {
                User = 2,
                Date = new DateTime(2020, 01, 01),
                Value = 8M
            });

            context.SaveChanges();

            var calculator = new FlexhourCalculator(context);
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Contains(flexhours, hour => hour.Value == 2.5M);
        }

        [Fact]
        public void GetFlexhoursToday_YouWorked10YesterdayAnd10HoursToday_2AndAHalfHoursInFlex()
        {
            var context = new AlvTimeDbContextBuilder().WithUsers().CreateDbContext();

            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 01, 01),
                Value = 10M
            });

            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2019, 12, 31),
                Value = 10M
            });

            context.SaveChanges();

            var calculator = new FlexhourCalculator(context);
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Single(flexhours);
            Assert.Contains(flexhours, hour => hour.Value == 2.5M);
        }

        [Fact]
        public void GetFlexhoursToday_YouWorked10YesterdayAnd10HoursToday_5HoursInFlex()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 01, 01),
                Value = 10M
            });

            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2019, 12, 31),
                Value = 10M
            });

            context.SaveChanges();

            var calculator = new FlexhourCalculator(context);
            var flexhours = calculator.GetFlexihours(new DateTime(2019, 12, 31), new DateTime(2020, 01, 01), 1);

            Assert.Equal(5.0M, flexhours.Sum(item => item.Value));
        }

        [Fact]
        public void GetFlexhours_Worked15HoursOneDayAndTakeOneDayOff_NoFlexHours()
        {
            var context = new AlvTimeDbContextBuilder()
                .WithUsers()
                .CreateDbContext();

            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 01, 01),
                Task = new Task
                {
                    Id = 13
                },
                Value = 15.0M
            });

            context.Hours.Add(new Hours
            {
                User = 1,
                Date = new DateTime(2020, 01, 02),
                Task = new Task
                {
                    Id = 14
                },
                Value = 7.5M
            });

            context.SaveChanges();

            var calculator = new FlexhourCalculator(context);
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 02), 1);

            Assert.Equal(2, flexhours.Count());
            Assert.Equal(7.5M, flexhours.First().Value);
            Assert.Equal(-7.5M, flexhours.Last().Value);
            Assert.Equal(0M, flexhours.Sum(item => item.Value));
        }
    }
}

internal class FlexhourCalculator : IFlexihourRepository
{
    private readonly AlvTime_dbContext _context;
    private readonly TimeEntryStorage _storage;

    public FlexhourCalculator(AlvTime_dbContext context)
    {
        _context = context;
        _storage = new TimeEntryStorage(_context);
    }

    public IEnumerable<FlexiHours> GetFlexihours(DateTime startDate, DateTime endDate, int userId)
    {
        var flexHours = new List<FlexiHours>();

        var timeEntries = _storage.GetTimeEntries(new TimeEntryQuerySearch
        {
            UserId = userId,
            FromDateInclusive = startDate,
            ToDateInclusive = endDate
        });

        var hoursByDate = timeEntries.GroupBy(
            h => h.Date,
            h => h,
            (date, entry) => new DateEntry
            {
                Date = date,
                Entries = entry.Select(e => new Entry
                {
                    TaskId = e.TaskId,
                    Value = e.Value
                })
            });

        for (DateTime d = startDate; d <= endDate; d += TimeSpan.FromDays(1))
        {
            var day = hoursByDate.SingleOrDefault(dd => DateTime.Parse(dd.Date) == d);
            if (day == null)
            {
                flexHours.Add(new FlexiHours
                {
                    Value = -7.5M,
                });
            }
            else if (day.GetWorkingHours() != 7.5M)
            {
                flexHours.Add(new FlexiHours
                {
                    Value = day.GetWorkingHours() - 7.5M,
                    Date = DateTime.Parse(day.Date)
                });
            }
        }

        return flexHours;
    }

    class DateEntry
    {
        public string Date { get; set; }

        public IEnumerable<Entry> Entries { get; set; }

        public decimal GetWorkingHours()
        {
            return Entries.Where(e => e.TaskId != 14).Sum(e => e.Value);
        }
    }

    public class Entry
    {
        public decimal Value { get; set; }

        public int TaskId { get; set; }
    }
}