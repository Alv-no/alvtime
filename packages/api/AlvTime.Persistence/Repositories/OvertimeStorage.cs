using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Business.Overtime;
using AlvTime.Persistence.DataBaseModels;

namespace AlvTime.Persistence.Repositories
{
    public class OvertimeStorage : IOvertimeStorage
    {
        private readonly AlvTime_dbContext _context;

        public OvertimeStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public List<EarnedOvertimeDto> GetEarnedOvertime(OvertimeQueryFilter criterias)
        {
            var overtimeEntries = _context.EarnedOvertime.AsQueryable()
                .Filter(criterias)
                .Select(entry => new EarnedOvertimeDto
                {
                    Date = entry.Date,
                    Value = entry.Value,
                    CompensationRate = entry.CompensationRate,
                    UserId = entry.UserId
                })
                .ToList();
            return overtimeEntries;
        }

        public void StoreOvertime(List<OvertimeEntry> overtimeEntries, int userId)
        {
            _context.EarnedOvertime.AddRange(overtimeEntries.Select(entry => new EarnedOvertime
            {
                Date = entry.Date,
                UserId = userId,
                Value = entry.Hours,
                CompensationRate = entry.CompensationRate
            }));
            _context.SaveChanges();
        }

        public void DeleteOvertimeOnDate(DateTime date, int userId)
        {
            var earnedOvertimeOnDate = _context.EarnedOvertime.Where(ot => ot.Date.Date == date.Date && ot.UserId == userId);
            _context.RemoveRange(earnedOvertimeOnDate);
            _context.SaveChanges();
        }
    }
}