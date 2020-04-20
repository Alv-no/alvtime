using System;

namespace AlvTime.Business.Users
{
    public class CreateUserRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime StartDate { get; set; }
        public decimal? FlexiHours { get; set; }
    }
}
