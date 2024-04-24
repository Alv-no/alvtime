using Alvtime.Adminpanel.Client.Models;
using Alvtime.Adminpanel.Client.Requests;

namespace Alvtime.Adminpanel.Client.Mappers;

public static class EmploymentRateMapper
{
    public static EmploymentRateUpsertRequest MapToEmploymentRateUpsertRequest(this EmployeeEmploymentRateModel employmentRate)
    {
        return new EmploymentRateUpsertRequest
        {
            RatePercentage = employmentRate.RatePercentage,
            FromDateInclusive = employmentRate.FromDateInclusive,
            ToDateInclusive = employmentRate.ToDateInclusive
        };
    }
}