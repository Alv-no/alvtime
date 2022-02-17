using System.Collections.Generic;
using System.Linq;

namespace AlvTime.Business.HourRates
{
    public class HourRateService
    {
        private readonly IHourRateStorage _storage;

        public HourRateService(IHourRateStorage storage)
        {
            _storage = storage;
        }

        public HourRateResponseDto CreateHourRate(CreateHourRateDto hourRate)
        {
            HourRateQuerySearch criterias = new HourRateQuerySearch{
                FromDate = hourRate.FromDate,
                TaskId = hourRate.TaskId
            };

            if (!GetHourRates(criterias).Any())
            {
                _storage.CreateHourRate(hourRate);
            }
            else
            {
                _storage.UpdateHourRate(hourRate);
            }

            return GetHourRates(criterias).Single();
        }

        public IEnumerable<HourRateResponseDto> GetHourRates(HourRateQuerySearch criterias)
        {
            return _storage.GetHourRates(criterias);
        }
    }
}
