using System;

namespace AlvTime.Business.Users
{
    public class CreateUserDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime? StartDate { get; set; }
    }
}
