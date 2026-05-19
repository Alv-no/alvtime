using AlvTime.Business.Overtime;
using AlvTime.Business.Tasks;

namespace AlvTime.Business.CompensationRate;

public static class CompensationRateHelper
{
    public static decimal ResolveCompensationRate(CompensationType compensationType, bool imposed, SalaryModel salaryModel) =>
        compensationType switch
        {
            CompensationType.Billable when imposed                                 => CompensationRates.Imposed,
            CompensationType.Billable when salaryModel == SalaryModel.Static       => CompensationRates.BillableStaticModel,
            CompensationType.Billable when salaryModel == SalaryModel.InvoiceBased => CompensationRates.BillableInvoiceModel,
            CompensationType.Internal                                              => CompensationRates.Internal,
            _                                                                      => CompensationRates.Volunteer
        };
}
