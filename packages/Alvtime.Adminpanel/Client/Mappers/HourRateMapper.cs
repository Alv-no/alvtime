using Alvtime.Adminpanel.Client.Models;
using Alvtime.Adminpanel.Client.Requests;

namespace Alvtime.Adminpanel.Client.Mappers;

public static class HourRateMapper
{
    public static HourRateUpsertRequest MapToHourRateUpsertRequest(this HourRateModel hourRate)
    {
        return new HourRateUpsertRequest
        {
            FromDate = (DateTime)hourRate.FromDate!,
            Rate = hourRate.Rate
        };
    }
}