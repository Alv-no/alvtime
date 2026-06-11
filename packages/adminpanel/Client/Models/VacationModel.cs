namespace Alvtime.Adminpanel.Client.Models;

public class VacationOverviewReportModel
{
    public int UserId { get; set; }
    public VacationDaysModel? VacationDaysDto { get; set; }
}

public class VacationDaysModel
{
    public decimal AvailableVacationDays { get; set; }
    public decimal AvailableVacationDaysTransferredFromLastYear { get; set; }
    public decimal PlannedVacationDaysThisYear { get; set; }
    public decimal UsedVacationDaysThisYear { get; set; }
    public IList<VacationTransactionModel> PlannedTransactions { get; set; } = [];
    public IList<VacationTransactionModel> UsedTransactions { get; set; } = [];
}

public class VacationTransactionModel
{
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
}
