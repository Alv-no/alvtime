namespace AlvTime.Business.EconomyData
{
    public record OvertimePayoutRespons
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public string Date { get; init; }
        public decimal TotalPayout { get; init; }
        public int PaidOvertimeId { get; init; }
    }

}