using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AlvTimeWebApi.Dto;

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

        public UserResponseDto CreateUser(CreateUserRequest user)
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

        private IEnumerable<UserResponseDto> GetUser(CreateUserRequest user)
        {
            return _storage.GetUser(new UserQuerySearch
            {
                Email = user.Email,
                Name = user.Name,
            });
        }
    }
}
