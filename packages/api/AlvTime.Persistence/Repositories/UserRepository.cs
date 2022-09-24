using AlvTime.Business.Users;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Models;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using User = AlvTime.Persistence.DatabaseModels.User;

namespace AlvTime.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AlvTime_dbContext _context;

        public UserRepository(AlvTime_dbContext context)
        {
            _context = context;
        }

        public void AddUser(UserDto user)
        {
            var newUser = new User
            {
                Name = user.Name,
                Email = user.Email,
                StartDate = (DateTime)user.StartDate?.Date,
                EndDate = user.EndDate,
            };

            _context.User.Add(newUser);
            _context.SaveChanges();
        }

        public async Task<IEnumerable<UserResponseDto>> GetUsers(UserQuerySearch criteria)
        {
            return await _context.User.AsQueryable()
                .Filter(criteria)
                .Select(u => new UserResponseDto
                {
                    Email = u.Email,
                    Id = u.Id,
                    Name = u.Name,
                    StartDate = u.StartDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    EndDate = u.EndDate != null
                        ? ((DateTime)u.EndDate).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                        : null
                }).ToListAsync();
        }

        public void UpdateUser(UserDto userToBeUpdated)
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
                    Name = databaseUser.Name,
                    EndDate = databaseUser.EndDate
                };
            }

            return null;
        }

        public async Task<EmploymentRateResponse> CreateEmploymentRateForUser(EmploymentRateDto input)
        {
            var rate = new EmploymentRate
            {
                UserId = input.UserId,
                FromDate = input.FromDateInclusive,
                ToDate = input.ToDateInclusive,
                Rate = input.Rate
            };
            _context.EmploymentRate.Add(rate);
            await _context.SaveChangesAsync();

            return new EmploymentRateResponse
            {
                Id = rate.Id,
                UserId = rate.UserId,
                Rate = rate.Rate,
                FromDateInclusive = rate.FromDate,
                ToDateInclusive = rate.ToDate
            };
        }

        public async Task<IEnumerable<EmploymentRateResponse>> GetEmploymentRates(EmploymentRateQueryFilter criteria)
        {
            var rates = await _context.EmploymentRate.AsQueryable()
                .Filter(criteria)
                .Select(er => new EmploymentRateResponse
            {
                Id = er.Id,
                UserId = er.Id,
                Rate = er.Rate,
                FromDateInclusive = er.FromDate,
                ToDateInclusive = er.ToDate
            }).ToListAsync();
            return rates;
        }

        public async Task<EmploymentRateResponse> UpdateEmploymentRateForUser(EmploymentRateChangeRequest request)
        {
            var existingRate = await _context.EmploymentRate.FindAsync(request.RateId);
            if (existingRate == null)
            {
                throw new SqlNullValueException("Rate was not found");
            }

            existingRate.Rate = request.Rate;
            existingRate.FromDate = request.FromDateInclusive;
            existingRate.ToDate = request.ToDateInclusive;

            await _context.SaveChangesAsync();
            return new EmploymentRateResponse
            {
                Id = existingRate.Id,
                UserId = existingRate.UserId,
                Rate = existingRate.Rate,
                FromDateInclusive = existingRate.FromDate,
                ToDateInclusive = existingRate.ToDate
            };
        }
    }
}