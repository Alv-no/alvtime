namespace Alvtime.Adminpanel.Client.Models;

public class EmployeeModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int EmployeeId { get; set; }
    public bool ShowDetails { get; set; }
    public IList<EmployeeEmploymentRateModel>? EmploymentRates { get; set; }
}

public class EmployeeEmploymentRateModel : IEquatable<EmployeeEmploymentRateModel>
{
    public int Id { get; set; }
    public decimal RatePercentage { get; set; }
    public DateTime? FromDateInclusive { get; set; }
    public DateTime? ToDateInclusive { get; set; }
    
    public bool Equals(EmployeeEmploymentRateModel? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((EmployeeEmploymentRateModel)obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}