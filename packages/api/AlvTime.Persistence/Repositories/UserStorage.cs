using AlvTime.Business.Users;
using AlvTime.Persistence.DataBaseModels;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Models;
using Microsoft.EntityFrameworkCore;
using User = AlvTime.Persistence.DataBaseModels.User;

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
                StartDate = (DateTime)user.StartDate?.Date,
                EndDate = user.EndDate ?? null
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

        public async Task<Business.Models.User> GetUserFromToken(Token token)
        {
            var databaseToken = await _context.AccessTokens.FirstOrDefaultAsync(x => x.Value == token.Value && x.ExpiryDate >= DateTime.UtcNow);

            if (databaseToken != null)
            {
                var databaseUser = await _context.User.FirstOrDefaultAsync(x => x.Id == databaseToken.UserId);

                return new Business.Models.User
                {
                    Id = databaseUser.Id,
                    Email = databaseUser.Email,
                    Name = databaseUser.Name
                };
            }

            return null;
        }
    }
}