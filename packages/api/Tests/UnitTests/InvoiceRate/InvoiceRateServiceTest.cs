using AlvTime.Business.Absence;
using AlvTime.Business.InvoiceRate;
using AlvTime.Business.Options;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static AlvTime.Business.InvoiceRate.InvoiceStatisticsDto;

namespace Tests.UnitTests.InvoiceRate;

public class InvoiceRateServiceTest
{
    private readonly IOptionsMonitor<TimeEntryOptions> _options;
    private readonly IRedDaysService _redDaysService;
    private readonly Mock<IUserContext> _userContextMock;
    
    public InvoiceRateServiceTest()
    {
        var entryOptions = new TimeEntryOptions
        {
            PaidHolidayTask = 10,
        };

        _options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);

        _redDaysService = new RedDaysService();

        _userContextMock = new Mock<IUserContext>();

        var user = new AlvTime.Business.Users.User
        {
            Id = 1,
            Email = "someone@alv.no",
            Name = "Someone",
            Oid = "12345678-1234-1234-1234-123456789012"
        };
        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(System.Threading.Tasks.Task.FromResult(user));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetInvoicePercentage_Without_Vacation()
    {
        var hours = new List<Hours>
        {
            new()
            {
                User = 1,
                Date = new DateTime(2022, 01, 03),
                DayNumber = (short)new DateTime(2022, 01, 03).DayOfYear,
                Id = 1,
                Locked = false,
                TaskId = 3,
                Value = 1.5m,
                Year = (short)new DateTime(2022, 01, 03).Year
            },
            new()
            {
                User = 1,
                Date = new DateTime(2022, 01, 03),
                DayNumber = (short)new DateTime(2022, 01, 03).DayOfYear,
                Id = 2,
                Locked = false,
                TaskId = 1,
                Value = 6m,
                Year = (short)new DateTime(2022, 01, 03).Year
            }
        };

        var service = CreateInvoiceRateService(hours);

        var invoiceRate = await service.GetEmployeeInvoiceRateForPeriod(new DateTime(2022, 01, 03), new DateTime(2022, 01, 03));

        Assert.Equal(0.8m, invoiceRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetInvoicePercentage_DuringEaster_WithVacation()
    {
        // One vacation-day on wednesday in easter and works half days monday and tuesday with billable
        var hours = new List<Hours>
        {
            new Hours
            {
                User = 1,
                Date = new DateTime(2022, 04, 11),
                DayNumber = (short)new DateTime(2022, 04, 11).DayOfYear,
                Id = 1,
                Locked = false,
                TaskId = 3,
                Value = 3m,
                Year = (short)new DateTime(2022, 04, 11).Year
            },
            new Hours
            {
                User = 1,
                Date = new DateTime(2022, 04, 11),
                DayNumber = (short)new DateTime(2022, 04, 11).DayOfYear,
                Id = 2,
                Locked = false,
                TaskId = 1,
                Value = 4.5m,
                Year = (short)new DateTime(2022, 04, 11).Year
            },
            new Hours
            {
                User = 1,
                Date = new DateTime(2022, 04, 12),
                DayNumber = (short)new DateTime(2022, 04, 12).DayOfYear,
                Id = 3,
                Locked = false,
                TaskId = 3,
                Value = 3m,
                Year = (short)new DateTime(2022, 04, 12).Year
            },
            new Hours
            {
                User = 1,
                Date = new DateTime(2022, 04, 12),
                DayNumber = (short)new DateTime(2022, 04, 12).DayOfYear,
                Id = 4,
                Locked = false,
                TaskId = 1,
                Value = 4.5m,
                Year = (short)new DateTime(2022, 04, 12).Year
            },
            new Hours
            {
                User = 1,
                Date = new DateTime(2022, 04, 13),
                DayNumber = (short)new DateTime(2022, 04, 13).DayOfYear,
                Id = 5,
                Locked = false,
                TaskId = 10,
                Value = 7.5m,
                Year = (short)new DateTime(2022, 04, 13).Year
            }
        };

        var service = CreateInvoiceRateService(hours);

        var invoiceRate = await service.GetEmployeeInvoiceRateForPeriod(new DateTime(2022, 4, 11), new DateTime(2022, 4, 17));

        Assert.Equal(0.6m, invoiceRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetInvoicePercentage_With_Weekend()
    {
        var hours = new List<Hours>
        {
            new Hours
            {
                User = 1,
                Date = new DateTime(2022, 12, 12),
                DayNumber = (short)new DateTime(2022, 12, 12).DayOfYear,
                Id = 1,
                Locked = false,
                TaskId = 1,
                Value = 7.5m,
                Year = (short)new DateTime(2022, 12, 12).Year
            },
            new Hours
            {
                User = 1,
                Date = new DateTime(2022, 12, 13),
                DayNumber = (short)new DateTime(2022, 12, 13).DayOfYear,
                Id = 2,
                Locked = false,
                TaskId = 1,
                Value = 7.5m,
                Year = (short)new DateTime(2022, 12, 13).Year
            },
            new Hours
            {
                User = 1,
                Date = new DateTime(2022, 12, 14),
                DayNumber = (short)new DateTime(2022, 12, 14).DayOfYear,
                Id = 3,
                Locked = false,
                TaskId = 1,
                Value = 7.5m,
                Year = (short)new DateTime(2022, 12, 14).Year
            },
            new Hours
            {
                User = 1,
                Date = new DateTime(2022, 12, 15),
                DayNumber = (short)new DateTime(2022, 12, 15).DayOfYear,
                Id = 4,
                Locked = false,
                TaskId = 1,
                Value = 7.5m,
                Year = (short)new DateTime(2022, 12, 15).Year
            },
            new Hours
            {
                User = 1,
                Date = new DateTime(2022, 12, 16),
                DayNumber = (short)new DateTime(2022, 12, 16).DayOfYear,
                Id = 5,
                Locked = false,
                TaskId = 1,
                Value = 7.5m,
                Year = (short)new DateTime(2022, 12, 16).Year
            } 
        };

        var service = CreateInvoiceRateService(hours);

        var invoiceRate = await service.GetEmployeeInvoiceRateForPeriod(new DateTime(2022, 12, 12), new DateTime(2022, 12, 18));

        Assert.Equal(1m, invoiceRate);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEmployeeInvoiceStatisticsByDaysForJanuaryWithoutIncludeZero_Expect21Items()
    {
        var startDate = new DateTime(2022, 1, 1);
        var endDate = new DateTime(2022, 1, 31);

        var hours = new List<Hours>();

        for (var i = 0; i < 59; i++)
        {
            var newDate = startDate.AddDays(i);
            if (newDate.DayOfWeek == DayOfWeek.Sunday || newDate.DayOfWeek == DayOfWeek.Saturday)
            {
                continue;
            }

            hours.Add(new Hours
            {
                User = 1,
                Date = newDate,
                DayNumber = (short)newDate.DayOfYear,
                Id = i + 1,
                Locked = false,
                TaskId = 1,
                Value = 7.5m,
                Year = (short)newDate.Year
            });
        }

        var service = CreateInvoiceRateService(hours);

        var statistics = await service.GetEmployeeInvoiceStatisticsByPeriod(startDate, endDate, InvoicePeriods.Daily, ExtendPeriod.None, false);

        Assert.Equal(21, statistics.BillableHours.Length);
        Assert.Equal(21, statistics.NonBillableHours.Length);
        Assert.Equal(21, statistics.VacationHours.Length);
        Assert.Equal(21, statistics.InvoiceRate.Length);
        Assert.Equal(21, statistics.NonBillableInvoiceRate.Length);
        Assert.Equal(21, statistics.Start.Length);
        Assert.Equal(21, statistics.End.Length);

        Assert.Equal(7.5m, statistics.BillableHours[0]);
        Assert.Equal(0m, statistics.VacationHours[0]);
        Assert.Equal(0m, statistics.NonBillableHours[0]);
        Assert.Equal(1m, statistics.InvoiceRate[0]);
        Assert.Equal(0m, statistics.NonBillableInvoiceRate[0]);
        Assert.Equal(new DateTime(2022, 1, 3), statistics.Start[0]);
        Assert.Equal(new DateTime(2022, 1, 3).Date, statistics.End[0].Date);

        Assert.Equal(7.5m, statistics.BillableHours.Last());
        Assert.Equal(0m, statistics.VacationHours.Last());
        Assert.Equal(0m, statistics.NonBillableHours.Last());
        Assert.Equal(1m, statistics.InvoiceRate.Last());
        Assert.Equal(0m, statistics.NonBillableInvoiceRate.Last());
        Assert.Equal(endDate, statistics.Start.Last().Date);
        Assert.Equal(endDate.Date, statistics.End.Last().Date);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEmployeeInvoiceStatisticsByDaysForJanuaryWithIncludeZero_Expect31Items()
    {
        var startDate = new DateTime(2022, 1, 1);
        var endDate = new DateTime(2022, 1, 31);

        var hours = new List<Hours>();

        for (var i = 0; i < 59; i++)
        {
            var newDate = startDate.AddDays(i);
            if (newDate.DayOfWeek == DayOfWeek.Sunday || newDate.DayOfWeek == DayOfWeek.Saturday)
            {
                continue;
            }

            hours.Add(new Hours
            {
                User = 1,
                Date = newDate,
                DayNumber = (short)newDate.DayOfYear,
                Id = i + 1,
                Locked = false,
                TaskId = 1,
                Value = 7.5m,
                Year = (short)newDate.Year
            });
        }

        var service = CreateInvoiceRateService(hours);

        var statistics = await service.GetEmployeeInvoiceStatisticsByPeriod(startDate, endDate, InvoicePeriods.Daily, ExtendPeriod.None, true);

        Assert.Equal(31, statistics.BillableHours.Count());
        Assert.Equal(31, statistics.NonBillableHours.Count());
        Assert.Equal(31, statistics.VacationHours.Count());
        Assert.Equal(31, statistics.InvoiceRate.Count());
        Assert.Equal(31, statistics.NonBillableInvoiceRate.Count());
        Assert.Equal(31, statistics.Start.Count());
        Assert.Equal(31, statistics.End.Count());

        Assert.Equal(7.5m, statistics.BillableHours[2]);
        Assert.Equal(0m, statistics.VacationHours[2]);
        Assert.Equal(0m, statistics.NonBillableHours[2]);
        Assert.Equal(1m, statistics.InvoiceRate[2]);
        Assert.Equal(0m, statistics.NonBillableInvoiceRate[2]);
        Assert.Equal(new DateTime(2022, 1, 3), statistics.Start[2]);
        Assert.Equal(new DateTime(2022, 1, 3).Date, statistics.End[2].Date);

        Assert.Equal(7.5m, statistics.BillableHours.Last());
        Assert.Equal(0m, statistics.VacationHours.Last());
        Assert.Equal(0m, statistics.NonBillableHours.Last());
        Assert.Equal(1m, statistics.InvoiceRate.Last());
        Assert.Equal(0m, statistics.NonBillableInvoiceRate.Last());
        Assert.Equal(endDate, statistics.Start.Last().Date);
        Assert.Equal(endDate.Date, statistics.End.Last().Date);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEmployeeInvoiceStatisticsForOneWeek_Expect1ItemAndCorrectHours()
    {
        var startDate = new DateTime(2022, 1, 3);
        var endDate = new DateTime(2022, 1, 9);

        var hours = new List<Hours>();
        for (var i = 0; i < 7; i++)
        {
            var newDate = startDate.AddDays(i);
            hours.Add(new Hours
            {
                User = 1,
                Date = newDate,
                DayNumber = (short)newDate.DayOfYear,
                Id = i + 1,
                Locked = false,
                TaskId = 1,
                Value = 7.5m,
                Year = (short)newDate.Year
            });
        }

        var service = CreateInvoiceRateService(hours);

        var statistics = await service.GetEmployeeInvoiceStatisticsByPeriod(startDate, endDate, InvoicePeriods.Weekly, ExtendPeriod.None, false);

        Assert.Single(statistics.BillableHours);
        Assert.Single(statistics.NonBillableHours);
        Assert.Single(statistics.VacationHours);
        Assert.Single(statistics.InvoiceRate);
        Assert.Single(statistics.NonBillableInvoiceRate);
        Assert.Single(statistics.Start);
        Assert.Single(statistics.End);

        Assert.Equal(7.5m * 7, statistics.BillableHours[0]);
        Assert.Equal(0m, statistics.VacationHours[0]);
        Assert.Equal(0m, statistics.NonBillableHours[0]);
        Assert.Equal(1.4m, statistics.InvoiceRate[0]);
        Assert.Equal(0m, statistics.NonBillableInvoiceRate[0]);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEmployeeInvoiceStatisticsForTwoWeeks_Expect2ItemsAndCorrectHours()
    {
        var startDate = new DateTime(2022, 1, 3);
        var endDate = new DateTime(2022, 1, 16);

        var hours = new List<Hours>();
        for (int i = 0; i < 14; i++)
        {
            var newDate = startDate.AddDays(i);
            if (newDate.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday)
            {
                continue;
            }
            hours.Add(new Hours
            {
                User = 1,
                Date = newDate,
                DayNumber = (short)newDate.DayOfYear,
                Id = i + 1,
                Locked = false,
                TaskId = 1,
                Value = 7.5m,
                Year = (short)newDate.Year
            });
        }

        var service = CreateInvoiceRateService(hours);

        var statistics = await service.GetEmployeeInvoiceStatisticsByPeriod(startDate, endDate, InvoicePeriods.Weekly, ExtendPeriod.None, false);

        Assert.Equal(2, statistics.BillableHours.Count());
        Assert.Equal(2, statistics.NonBillableHours.Count());
        Assert.Equal(2, statistics.VacationHours.Count());
        Assert.Equal(2, statistics.InvoiceRate.Count());
        Assert.Equal(2, statistics.NonBillableInvoiceRate.Count());
        Assert.Equal(2, statistics.Start.Count());
        Assert.Equal(2, statistics.End.Count());

        Assert.Equal(7.5m * 5, statistics.BillableHours[0]);
        Assert.Equal(0m, statistics.VacationHours[0]);
        Assert.Equal(0m, statistics.NonBillableHours[0]);
        Assert.Equal(1m, statistics.InvoiceRate[0]);
        Assert.Equal(0m, statistics.NonBillableInvoiceRate[0]);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEmployeeInvoiceStatisticsFor12Months_Expect12Periods()
    {
        var startDate = new DateTime(2022, 1, 1);
        var endDate = new DateTime(2022, 12, 31);

        var hours = new List<Hours>();
        for (var i = 0; i < 365; i++)
        {
            var newDate = startDate.AddDays(i);
            if (newDate.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday)
            {
                continue;
            }
            hours.Add(new Hours
            {
                User = 1,
                Date = newDate,
                DayNumber = (short)newDate.DayOfYear,
                Id = i + 1,
                Locked = false,
                TaskId = 1,
                Value = 7.5m,
                Year = (short)newDate.Year
            });
        }

        var service = CreateInvoiceRateService(hours);

        var statistics = await service.GetEmployeeInvoiceStatisticsByPeriod(startDate, endDate, InvoicePeriods.Monthly, ExtendPeriod.None, false);

        Assert.Equal(12, statistics.BillableHours.Count());
        Assert.Equal(12, statistics.NonBillableHours.Count());
        Assert.Equal(12, statistics.VacationHours.Count());
        Assert.Equal(12, statistics.InvoiceRate.Count());
        Assert.Equal(12, statistics.NonBillableInvoiceRate.Count());
        Assert.Equal(12, statistics.Start.Count());
        Assert.Equal(12, statistics.End.Count());

        Assert.Equal(1m, statistics.InvoiceRate[0]);
        Assert.Equal(startDate, statistics.Start[0]);
        Assert.Equal(new DateTime(2022, 1, 31), statistics.End[0].Date);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEmployeeInvoiceStatisticsFor2MonthsFromMidToMidOfMonth_Expect2Periods()
    {
        var startDate = new DateTime(2022, 1, 15);
        var endDate = new DateTime(2022, 2, 15);

        var hours = new List<Hours>();
        for (var i = 0; i < 31; i++)
        {
            var newDate = startDate.AddDays(i);
            if (newDate.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday)
            {
                continue;
            }
            hours.Add(new Hours
            {
                User = 1,
                Date = newDate,
                DayNumber = (short)newDate.DayOfYear,
                Id = i + 1,
                Locked = false,
                TaskId = 1,
                Value = 7.5m,
                Year = (short)newDate.Year
            });
        }

        var service = CreateInvoiceRateService(hours);

        var statistics = await service.GetEmployeeInvoiceStatisticsByPeriod(startDate, endDate, InvoicePeriods.Monthly, ExtendPeriod.None, true);

        Assert.Equal(2, statistics.BillableHours.Length);
        Assert.Equal(2, statistics.NonBillableHours.Length);
        Assert.Equal(2, statistics.VacationHours.Length);
        Assert.Equal(2, statistics.InvoiceRate.Length);
        Assert.Equal(2, statistics.NonBillableInvoiceRate.Length);
        Assert.Equal(2, statistics.Start.Length);
        Assert.Equal(2, statistics.End.Length);

        Assert.Equal(1m, statistics.InvoiceRate[0]);
        Assert.Equal(new DateTime(2022, 1, 1), statistics.Start[0]);
        Assert.Equal(new DateTime(2022, 1, 31), statistics.End[0].Date);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetEmployeeInvoiceStatisticsFor2Years_Expect12Periods()
    {
        var startDate = new DateTime(2022, 1, 1);
        var endDate = new DateTime(2023, 12, 31);

        var hours = new List<Hours>();
        for (var i = 0; i < 730; i++)
        {
            var newDate = startDate.AddDays(i);
            if (newDate.DayOfWeek is DayOfWeek.Sunday or DayOfWeek.Saturday)
            {
                continue;
            }
            hours.Add(new Hours
            {
                User = 1,
                Date = newDate,
                DayNumber = (short)newDate.DayOfYear,
                Id = i + 1,
                Locked = false,
                TaskId = 1,
                Value = 7.5m,
                Year = (short)newDate.Year
            });
        }

        var service = CreateInvoiceRateService(hours);

        var statistics = await service.GetEmployeeInvoiceStatisticsByPeriod(startDate, endDate, InvoicePeriods.Annually, ExtendPeriod.None, false);

        Assert.Equal(2, statistics.BillableHours.Length);
        Assert.Equal(2, statistics.NonBillableHours.Length);
        Assert.Equal(2, statistics.VacationHours.Length);
        Assert.Equal(2, statistics.InvoiceRate.Length);
        Assert.Equal(2, statistics.NonBillableInvoiceRate.Length);
        Assert.Equal(2, statistics.Start.Length);
        Assert.Equal(2, statistics.End.Length);

        Assert.True(1m < statistics.InvoiceRate[0]);
        Assert.Equal(startDate, statistics.Start[0]);
        Assert.Equal(new DateTime(2022, 12, 31), statistics.End[0].Date);
    }

    private InvoiceRateService CreateInvoiceRateService(List<Hours> hours)
    {
        var context = new AlvTimeDbContextBuilder()
            .CreateDbContext();
        PopulateContext(context, hours);
        ITimeRegistrationStorage storage = new TimeRegistrationStorage(context);
        return new InvoiceRateService(storage, _redDaysService, _options, _userContextMock.Object);
    }

    private void PopulateContext(AlvTime_dbContext context, List<Hours> hours)
    {
        foreach (var hour in hours)
            context.Hours.Add(hour);

        context.HourRate.Add(new HourRate
        {
            Id = 1,
            FromDate = new DateTime(2019, 01, 01),
            Rate = 1000,
            TaskId = 1
        });

        context.Customer.Add(new Customer
        {
            Id = 1,
            Name = "Alv"
        });
        context.Customer.Add(new Customer
        {
            Id = 2,
            Name = "Evil inc"
        });

        context.Project.Add(new Project
        {
            Id = 1,
            Name = "Internal",
            Customer = 1
        });
        context.Project.Add(new Project
        {
            Id = 2,
            Name = "Money Maker",
            Customer = 2
        });
        context.Project.Add(new Project
        {
            Id = 9,
            Name = "Absence Project",
            Customer = 1
        });

        context.Task.Add(new Task
        {
            Id = 1,
            Description = "",
            Project = 2,
            Name = "Print Money"
        });

        context.Task.Add(new Task
        {
            Id = 2,
            Description = "",
            Project = 1,
            Name = "Slave Labor"
        });

        context.Task.Add(new Task
        {
            Id = 3,
            Description = "",
            Project = 1,
            Name = "Make Lunch"
        });

        context.CompensationRate.Add(new CompensationRate
        {
            TaskId = 1,
            Value = 1.5M,
            FromDate = new DateTime(2019, 01, 01)
        });
        context.CompensationRate.Add(new CompensationRate
        {
            TaskId = 2,
            Value = 1.0M,
            FromDate = new DateTime(2019, 01, 01)
        });
        context.CompensationRate.Add(new CompensationRate
        {
            TaskId = 3,
            Value = 0.5M,
            FromDate = new DateTime(2019, 01, 01)
        });

        context.Task.Add(new Task
        {
            Id = 10,
            Description = "",
            Project = 9,
            Name = "PaidHoliday",
            Locked = false
        });
        context.Task.Add(new Task
        {
            Id = 11,
            Description = "",
            Project = 9,
            Name = "SickDay",
            Locked = false
        });

        context.CompensationRate.Add(new CompensationRate
        {
            TaskId = 10,
            Value = 1.0M,
            FromDate = new DateTime(2019, 01, 01)
        });
        context.CompensationRate.Add(new CompensationRate
        {
            TaskId = 11,
            Value = 1.0M,
            FromDate = new DateTime(2019, 01, 01)
        });

        context.SaveChanges();
    }
}
