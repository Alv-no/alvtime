using System;

namespace AlvTime.Business.Users;

public interface ISalaryRepository
{
}

public class SalaryQuerySearch
{
    public int? UserId { get; set; }
    public string Email { get; set; }
    public DateTime? Date { get; set; }
}