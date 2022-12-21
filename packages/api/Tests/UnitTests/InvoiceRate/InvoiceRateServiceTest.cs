using System;
using System.Threading.Tasks;
using Xunit;
using AlvTime.Business.Options;
using Moq;
using Microsoft.Extensions.Options;
using AlvTime.Business.Holidays;
using AlvTime.Business.InvoiceRate;
using System.Collections.Generic;
using AlvTime.Business.TimeEntries;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Interfaces;

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
            PaidHolidayTask = 13,
        };

        _options = Mock.Of<IOptionsMonitor<TimeEntryOptions>>(options => options.CurrentValue == entryOptions);

        _redDaysService = new RedDaysService();

        _userContextMock = new Mock<IUserContext>();

        var user = new AlvTime.Business.Models.User
        {
            Id = 1,
            Email = "someone@alv.no",
            Name = "Someone"
        };


        _userContextMock.Setup(context => context.GetCurrentUser()).Returns(Task.FromResult(user));
    }

    [Fact]
    public async Task GetInvoicePercentage_Without_Vacation()
    {
        var mockRepository = CreateMockRepository(new List<TimeEntryWithCustomerDto>{
                    new TimeEntryWithCustomerDto {
                        TaskId = 1,
                        CustomerName = "alv",
                        Value = 1.5m,
                        Date = new DateTime(2022, 01, 01)
                    },
                    new TimeEntryWithCustomerDto {
                        TaskId = 2,
                        CustomerName = "evil inc",
                        Value = 6,
                        Date = new DateTime(2022, 01, 01)
                    }
                });

        var service = new InvoiceRateService(mockRepository.Object, _redDaysService, _options, _userContextMock.Object);

        decimal invoiceRate = await service.GetEmployeeInvoiceRateForPeriod(new DateTime(2022, 01, 03), new DateTime(2022, 01, 03));

        Assert.Equal(0.8m, invoiceRate);
    }

    [Fact]
    public async Task GetInvoicePercentage_DuringEaster_WithVacation()
    {
        // One vacation-day on wednesday in easter and works half days monday and tuesday with billable
        var mockRepository = CreateMockRepository(new List<TimeEntryWithCustomerDto>{
                    new TimeEntryWithCustomerDto {
                        TaskId = 1,
                        CustomerName = "alv",
                        Value = 3m,
                        Date = new DateTime(2022, 04, 11)
                    },
                    new TimeEntryWithCustomerDto {
                        TaskId = 1,
                        CustomerName = "alv",
                        Value = 3m,
                        Date = new DateTime(2022, 04, 12)
                    },
                    new TimeEntryWithCustomerDto {
                        TaskId = 2,
                        CustomerName = "evil inc",
                        Value = 4.5m,
                        Date = new DateTime(2022, 04, 11)
                    },
                    new TimeEntryWithCustomerDto
                    {
                        TaskId = 2,
                        CustomerName = "evil inc",
                        Value = 4.5m,
                        Date = new DateTime(2022, 04, 12)
                    },
                    new TimeEntryWithCustomerDto {
                        TaskId = 13,
                        CustomerName = "alv",
                        Value = 7.5m,
                        Date = new DateTime(2022, 04, 13)
                    },
                });

        var service = new InvoiceRateService(mockRepository.Object, _redDaysService, _options, _userContextMock.Object);

        decimal invoiceRate = await service.GetEmployeeInvoiceRateForPeriod(new DateTime(2022, 4, 11), new DateTime(2022, 4, 17));

        Assert.Equal(0.6m, invoiceRate);
    }

    [Fact]
    public async Task GetInvoicePercentage_With_Weekend()
    {
        // One vacation-day on wednesday in easter and works half days monday and tuesday with billable
        var mockRepository = CreateMockRepository(new List<TimeEntryWithCustomerDto>{
                    new TimeEntryWithCustomerDto {
                        TaskId = 2,
                        CustomerName = "evil inc",
                        Value = 7.5m,
                        Date = new DateTime(2022, 12, 12)
                    },
                    new TimeEntryWithCustomerDto {
                        TaskId = 2,
                        CustomerName = "evil inc",
                        Value = 7.5m,
                        Date = new DateTime(2022, 12, 13)
                    },
                    new TimeEntryWithCustomerDto {
                        TaskId = 2,
                        CustomerName = "evil inc",
                        Value = 7.5m,
                        Date = new DateTime(2022, 12, 14)
                    },
                    new TimeEntryWithCustomerDto {
                        TaskId = 2,
                        CustomerName = "evil inc",
                        Value = 7.5m,
                        Date = new DateTime(2022, 12, 15)
                    },
                    new TimeEntryWithCustomerDto {
                        TaskId = 2,
                        CustomerName = "evil inc",
                        Value = 7.5m,
                        Date = new DateTime(2022, 12, 16)
                    },
                });

        var service = new InvoiceRateService(mockRepository.Object, _redDaysService, _options, _userContextMock.Object);

        decimal invoiceRate = await service.GetEmployeeInvoiceRateForPeriod(new DateTime(2022, 12, 12), new DateTime(2022, 12, 18));

        Assert.Equal(1m, invoiceRate);
    }

    private Mock<ITimeRegistrationStorage> CreateMockRepository(List<TimeEntryWithCustomerDto> result)
    {
        var userInvoiceMock = new Mock<ITimeRegistrationStorage>();
        userInvoiceMock.Setup(context => context.GetTimeEntriesWithCustomer(It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>())).Returns(Task.FromResult(result));

        return userInvoiceMock;
    }
}
