using AlvTimeWebApi.Dto;
using System.Collections.Generic;

namespace AlvTimeWebApi.Controllers.Admin.Users
{
    public interface IUserStorage
    {
        IEnumerable<UserResponseDto> GetUser(UserQuerySearch criterias);
    }

    public class UserQuerySearch
    {
        public string Name { get; set; }

        public string Email { get; set; }
    }
}