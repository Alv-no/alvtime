using System;

namespace AlvTime.Business.EconomyData
{
    public class OvertimePayoutResponsDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Date { get; set; }
        public decimal TotalPayout { get; set; }
        public int PaidOvertimeId { get; set; }
    }
}