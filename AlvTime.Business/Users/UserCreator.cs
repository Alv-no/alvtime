using System.Collections.Generic;
using System.Linq;

namespace AlvTime.Business.Users
{
    public class UserCreator
    {
        private readonly IUserStorage _storage;
        private readonly AlvHoursCalculator _calculator;

        public UserCreator(IUserStorage storage, AlvHoursCalculator calculator)
        {
            _storage = storage;
            _calculator = calculator;
        }

        public UserResponseDto CreateUser(CreateUserDto user)
        {
            if (user.FlexiHours == null)
            {
                user.FlexiHours = 187.5M + _calculator.CalculateAlvHours();
            }

            if (!GetUser(user).Any())
            {
                _storage.AddUser(user);
            }

            return GetUser(user).Single();

        }

        private IEnumerable<UserResponseDto> GetUser(CreateUserDto user)
        {
            return _storage.GetUser(new UserQuerySearch
            {
                Email = user.Email,
                Name = user.Name,
            });
        }
    }
}
