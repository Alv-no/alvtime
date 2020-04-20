using System;

namespace AlvTimeWebApi.Dto
{
    public class CreateUserDto
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public DateTime StartDate { get; set; }
        public decimal? FlexiHours { get; set; }
    }
}
