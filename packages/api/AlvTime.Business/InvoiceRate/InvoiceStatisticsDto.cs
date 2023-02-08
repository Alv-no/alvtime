using System;

namespace AlvTime.Business.InvoiceRate;

public class InvoiceStatisticsDto
{
    public decimal[] BillableHours { get; set; }
    public decimal[] NonBillableHours { get; set; }
    public decimal[] VacationHours { get; set; }
    public decimal[] InvoiceRate { get; set; }
    public decimal[] NonBillableInvoiceRate { get; set; }
    public DateTime[] Start { get; set; }
    public DateTime[] End { get; set; }
    [Obsolete("This attribute will be removed in the next iteration of invoice-rate functionality. Use Start or End instead")]
    public DateTime[] Labels { get { return Start; } }


    public enum InvoicePeriods
    {
        Daily = 0,
        Weekly = 1,
        Monthly = 2,
        Annualy = 3
    }

    [Flags]
    public enum ExtendPeriod
    { 
        None = 0,
        Start = 1,
        End = 2
    }
}
