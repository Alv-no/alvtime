using AlvTime.Business.FlexiHours;
using AlvTime.Business.TimeEntries;
using AlvTime.Persistence.Repositories;
using AlvTimeWebApi.Persistence.DatabaseModels;
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
        public void GetFlexhours_NoOvertime_NoFlexhour()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

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
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

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
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

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
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

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
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

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
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

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
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

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
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

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
            var flexhours = calculator.GetFlexihours(new DateTime(2019, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.True(flexhours.Sum(item => item.Value) == 5.0M);
        }

        [Fact]
        public void GetFlexhoursToday_Worked10HoursOnBillableAnd10HoursOnNonBillable_2AndAHalfHoursInFlex()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            context.Hours.Add(new Hours
            {
                User = 1,
                Task = new Task
                {
                    Id = 1,
                    CompensationRate = 1.5M
                },
                Date = new DateTime(2020, 01, 01),
                Value = 10M
            });

            context.Hours.Add(new Hours
            {
                User = 1,
                Task = new Task
                {
                    Id = 2,
                    CompensationRate = 1.0M
                },
                Date = new DateTime(2020, 01, 02),
                Value = 10M
            });

            context.SaveChanges();

            Assert.True(context.Task.Any());

            var calculator = new FlexhourCalculator(context);
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 02), 1);

            Assert.Single(flexhours);
            Assert.Contains(flexhours, item => item.Value == 2.5M);
        }

        [Fact]
        public void GetFlexhoursToday_Recorded0HoursOnWeekend_EmptyFlexHours()
        {
            var context = new AlvTimeDbContextBuilder().CreateDbContext();

            context.Hours.Add(new Hours
            {
                User = 1,
                Task = new Task
                {
                    Id = 1
                },
                Date = new DateTime(2020, 01, 01),
                Value = 0M
            });

            context.SaveChanges();

            Assert.True(context.Task.Any());

            var calculator = new FlexhourCalculator(context);
            var flexhours = calculator.GetFlexihours(new DateTime(2020, 01, 01), new DateTime(2020, 01, 01), 1);

            Assert.Empty(flexhours);
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

            if (!_context.Hours.Any())
            {
                flexHours.Add(new FlexiHours
                {
                    Value = -7.5M
                });
            }
            else
            {
                var hoursByDate = timeEntries.GroupBy(
                    h => h.Date,
                    h => h.Value,
                    (date, value) => new
                    {
                        Date = date,
                        SumHours = value.ToList().Sum()
                    });

                foreach (var hour in hoursByDate)
                {

                    if (hour.SumHours != 7.5M)
                    {
                        flexHours.Add(new FlexiHours
                        {
                            Value = hour.SumHours - 7.5M,
                            Date = DateTime.Parse(hour.Date)
                        });
                    }
                }
            }

            return flexHours;
        }
    }
}
