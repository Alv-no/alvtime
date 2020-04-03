namespace AlvTimeWebApi.Dto
{
    public class UpdateCustomerDto
    {
        public int Id { get; set; }
        public string InvoiceAddress { get; set; }
        public string ContactPerson { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
    }
}
