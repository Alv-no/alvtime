namespace Alvtime.Adminpanel.Client.Models;

public class CustomerModel : IEquatable<CustomerModel>
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? InvoiceAddress { get; set; }
    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? OrgNr { get; set; }
    public IList<ProjectModel>? Projects { get; set; }
    public bool ShowDetails { get; set; }
    
    public bool Equals(CustomerModel? other)
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
        return Equals((CustomerModel)obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}

public class ProjectModel : IEquatable<ProjectModel>
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public IList<TaskModel>? Tasks { get; set; }
    public bool ShowDetails { get; set; }
    public int? TaskCount { get; set; }
    public int? EmployeeCount { get; set; }
    
    public bool Equals(ProjectModel? other)
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
        return Equals((ProjectModel)obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}

public class TaskModel : IEquatable<TaskModel>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string? Description { get; set; }
    public bool Locked { get; set; }
    public bool Imposed { get; set; }
    public decimal CompensationRate { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public IList<HourRateModel>? HourRates { get; set; }
    public int HourRateCount { get; set; }
    public bool ShowDetails { get; set; }
    
    public bool Equals(TaskModel? other)
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
        return Equals((TaskModel)obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }

}

public class HourRateModel : IEquatable<HourRateModel>
{
    public int Id { get; set; }
    public DateTime? FromDate { get; set; }
    public int TaskId { get; set; }
    public string TaskName { get; set; }
    public int ProjectId { get; set; }
    public string ProjectName { get; set; }
    public decimal Rate { get; set; }
    
    public bool Equals(HourRateModel? other)
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
        return Equals((HourRateModel)obj);
    }

    public override int GetHashCode()
    {
        return Id;
    }
}