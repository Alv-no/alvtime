using System.Collections.Generic;
using System.Linq;

namespace AlvTime.Business.Users
{
    public class UserService
    {
        private readonly IUserStorage _storage;

        public UserService(IUserStorage storage)
        {
            _storage = storage;
        }

        public UserResponseDto CreateUser(CreateUserDto user)
        {
            UserQuerySearch criterias = new UserQuerySearch { Email = user.Email, Name = user.Name };
            if (!GetUsers(criterias).Any())
            {
                _storage.AddUser(user);
            }

            return GetUsers(criterias).Single();
        }

        public UserResponseDto UpdateUser(CreateUserDto user)
        {
            _storage.UpdateUser(user);
            return GetUsers(new UserQuerySearch{
                Id = user.Id
            }).Single();
        }
        public IEnumerable<UserResponseDto> GetUsers(UserQuerySearch criterias)
        {
            return _storage.GetUser(criterias);
        }

    }
}
