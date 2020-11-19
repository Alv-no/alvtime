using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AlvTime.Business.HourRates
{
    public class HourRateCreator
    {
        private readonly IHourRateStorage _storage;

        public HourRateCreator(IHourRateStorage storage)
        {
            _storage = storage;
        }

        public HourRateResponseDto CreateHourRate(CreateHourRateDto hourRate)
        {
            if (!GetHourRate(hourRate).Any())
            {
                _storage.CreateHourRate(hourRate);
            }
            else
            {
                _storage.UpdateHourRate(hourRate);
            }

            return GetHourRate(hourRate).Single();
        }

        public IEnumerable<HourRateResponseDto> GetHourRate(CreateHourRateDto hourRate)
        {
            return _storage.GetHourRates(new HourRateQuerySearch
            {
                FromDate = hourRate.FromDate,
                TaskId = hourRate.TaskId
            });
        }
    }
}
