using AlvTime.Business.HourRates;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses.Admin;
using AlvTimeWebApi.Utils;

namespace AlvTimeWebApi.Controllers.Utils;

public static class HourRateMapper
{
    public static HourRateDto MapToHourRateDto(this HourRateUpsertRequest hourRate)
    {
        return new HourRateDto
        {
            FromDate = hourRate.FromDate,
            Rate = hourRate.Rate
        };
    }
    
    public static HourRateDto MapToHourRateDto(this HourRateUpsertRequest hourRate, int hourRateId)
    {
        return new HourRateDto
        {
            Id = hourRateId,
            FromDate = hourRate.FromDate,
            Rate = hourRate.Rate
        };
    }
    
    public static HourRateResponse MapToHourRateResponse(this HourRateDto hourRate)
    {
        return new HourRateResponse
        {
            Id = hourRate.Id,
            FromDate = hourRate.FromDate.ToDateOnly(),
            Rate = hourRate.Rate
        };
    }
}