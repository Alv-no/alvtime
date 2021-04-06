using AlvTime.Business.Users;
using AlvTime.Persistence.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AlvTime.Persistence.Repositories
{
    public class UserStorage : IUserStorage
    {
        private readonly AlvTime_dbContext _context;

        public UserStorage(AlvTime_dbContext context)
        {
            _context = context;
        }

        public void AddUser(CreateUserDto user)
        {
            var newUser = new User
            {
                Name = user.Name,
                Email = user.Email,
                StartDate = (DateTime)user.StartDate
            };

            _context.User.Add(newUser);
            _context.SaveChanges();
        }

        public IEnumerable<UserResponseDto> GetUser(UserQuerySearch criterias)
        {
            return _context.User.AsQueryable()
                .Filter(criterias)
                .Select(u => new UserResponseDto
                {
                    Email = u.Email,
                    Id = u.Id,
                    Name = u.Name,
                    StartDate = u.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    EndDate = u.EndDate != null ? ((DateTime)u.EndDate).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : null
                }).ToList();
        }

        public void UpdateUser(CreateUserDto userToBeUpdated)
        {
            var existingUser = _context.User.FirstOrDefault(u => u.Id == userToBeUpdated.Id);

            if (userToBeUpdated.Name != null)
            {
                existingUser.Name = userToBeUpdated.Name;
            }
            if (userToBeUpdated.Email != null)
            {
                existingUser.Email = userToBeUpdated.Email;
            }
            if (userToBeUpdated.StartDate != null)
            {
                existingUser.StartDate = (DateTime)userToBeUpdated.StartDate;
            }
            if (userToBeUpdated.EndDate != null)
            {
                existingUser.EndDate = (DateTime)userToBeUpdated.EndDate;
            }

            _context.SaveChanges();
        }
    }
}
