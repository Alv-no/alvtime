using System.Collections.Generic;

namespace AlvTime.Business.Users
{
    public interface IUserStorage
    {
        IEnumerable<UserResponseDto> GetUser(UserQuerySearch criterias);
        void AddUser(CreateUserDto user);
    }

    public class UserQuerySearch
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }
}