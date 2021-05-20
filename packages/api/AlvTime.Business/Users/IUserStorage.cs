using System;
using System.Collections.Generic;

namespace AlvTime.Business.Users
{
    public interface IUserStorage
    {
        IEnumerable<UserResponseDto> GetUser(UserQuerySearch criterias);
        void AddUser(CreateUserDto user);
        void UpdateUser(CreateUserDto user);
    }

    public class UserQuerySearch
    {
        public int? Id { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}