using System.Collections.Generic;

namespace AlvTimeWebApi.Responses.Admin;

public class UserAdminResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public int? EmployeeId { get; set; }
    public string ProfilePicture { get; set; }
    public IEnumerable<UserEmploymentRateAdminResponse> EmploymentRates { get; set; }
}

public class UserEmploymentRateAdminResponse
{
    public int Id { get; set; }
    public decimal RatePercentage { get; set; }
    public string FromDateInclusive { get; set; }
    public string ToDateInclusive { get; set; }
}