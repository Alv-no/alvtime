namespace AlvTime.Business.InvoiceRate;

public class InvoiceStatisticsDto
{
    public decimal[] BillableHours { get; set; }
    public decimal[] NonBillableHours { get; set; }
    public decimal[] VacationHours { get; set; }
    public decimal[] InvoiceRate { get; set; }
}
