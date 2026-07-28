namespace Alvtime.Adminpanel.Client.Requests;

public class LockCustomersRequest
{
    public DateTime ToDateInclusive { get; set; }
    public List<int> CustomersToExclude { get; set; } = [];
}
