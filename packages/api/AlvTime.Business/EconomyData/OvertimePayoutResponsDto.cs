using System;

namespace AlvTime.Business.EconomyData
{
    public class OvertimePayoutResponsDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public decimal TotalPayout { get; set; }
    }
}