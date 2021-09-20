using System;
using AlvTime.Business.Options;
using AlvTime.Business.Services;
using AlvTime.Persistence.DataBaseModels;
using AlvTime.Persistence.EconomyDataDBModels;
using AlvTime.Persistence.Repositories;
using AlvTime.Persistence.Repositories.AlvEconomyData;

namespace Tests.UnitTests.Flexihours
{
    public static class FlexiHoursTestUtils
    {
        public static Hours CreateTimeEntry(DateTime date, decimal value, out int taskId)
        {
            taskId = new Random().Next();

            return new Hours
            {
                User = 1,
                Date = date,
                Value = value,
                Task = new Task { Id = taskId }
            };
        }

        public static CompensationRate CreateCompensationRate(int taskId, decimal compRate)
        {
            return new CompensationRate
            {
                FromDate = DateTime.UtcNow,
                Value = compRate,
                TaskId = taskId
            };
        }
        public  static FlexhourStorage CreateStorage(AlvTime_dbContext _context, AlvEconomyDataContext _economyDataContext)
        {
            return new FlexhourStorage(timeEntryStorage: new TimeEntryStorage(_context),
                context: _context,
                timeEntryOptions: new GetOvertimeTests.TestTimeEntryOptions(
                    new TimeEntryOptions
                    {
                        FlexTask = 18,
                        ReportUser = 11,
                        StartOfOvertimeSystem = new DateTime(2020, 01, 01)
                    }),
                new SalaryService(new OvertimePayoutStorage(_economyDataContext), new EmployeeHourlySalaryStorage(_economyDataContext, _context)));
        }
    }
}
