using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.EconomyData;
using AlvTime.Business.FlexiHours;
using AlvTime.Persistence.EconomyDataDBModels;
using FluentValidation;

namespace AlvTime.Persistence.Repositories.AlvEconomyData
{
    public class OvertimePayoutStorage : IOvertimePayoutStorage
    {
        private readonly AlvEconomyDataContext _economyContext;

        public OvertimePayoutStorage(AlvEconomyDataContext economyContext)
        {
            _economyContext = economyContext;
        }

        public OvertimePayoutResponsDto DeleteOvertimePayout(int userId, DateTime date)
        {
            var overtimePayout =
                _economyContext.OvertimePayouts.FirstOrDefault(op => op.UserId == userId && op.Date.Equals(date));

            if (overtimePayout == null)
            {
                throw new ValidationException("Could not find a payout registered for this user");
            }

            _economyContext.OvertimePayouts.Remove(overtimePayout);
            _economyContext.SaveChanges();

            return new OvertimePayoutResponsDto
            {
                
                Id = overtimePayout.Id,
                Date = overtimePayout.Date,
                UserId = overtimePayout.UserId,
                TotalPayout = overtimePayout.TotalPayout
            };
        }

        public void SaveOvertimePayout(RegisterOvertimePayoutDto overtimePayout)
        {
            _economyContext.OvertimePayouts.Add(new OvertimePayout
            {
                UserId = overtimePayout.UserId,
                Date = overtimePayout.Date,
                TotalPayout = overtimePayout.TotalPayout
            });
            _economyContext.SaveChanges();
        }
        
        public List<OvertimeEntry> GetOvertimeEntriesForPayout(List<OvertimeEntry> overtimeEntries, decimal hoursForPayout)
        {
            var tempHourCounter = 0.0M;
            var overtimeEntriesForPayout = new List<OvertimeEntry>();
            var orderedOverTimeEntries = overtimeEntries.OrderBy(oe => oe.Date).ToList();
            var indexOverTimeEntries = 0;

            while (tempHourCounter < hoursForPayout)
            {
                tempHourCounter += orderedOverTimeEntries[indexOverTimeEntries].Hours;

                if (tempHourCounter > hoursForPayout)
                {
                    overtimeEntriesForPayout.Add(new OvertimeEntry
                    {
                        CompensationRate = orderedOverTimeEntries[indexOverTimeEntries].CompensationRate,
                        Date = orderedOverTimeEntries[indexOverTimeEntries].Date,
                        TaskId = orderedOverTimeEntries[indexOverTimeEntries].TaskId,
                        Hours = hoursForPayout - overtimeEntriesForPayout.Sum(oe => oe.Hours)
                    });

                }
                else
                {
                    overtimeEntriesForPayout.Add(orderedOverTimeEntries[indexOverTimeEntries]);
                }

                indexOverTimeEntries++;
            }

            return overtimeEntriesForPayout;
        }
    }
}