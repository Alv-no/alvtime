using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlvTime.Business.AccessTokens;

namespace AlvTime.Business.Users
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserResponseDto>> GetUsers(UserQuerySearch criteria);
        Task AddUser(UserDto user);
        Task UpdateUser(UserDto user);
        Task<User> GetUserFromToken(Token token);
        Task<EmploymentRateResponseDto> CreateEmploymentRateForUser(EmploymentRateDto input);
        Task<IEnumerable<EmploymentRateResponseDto>> GetEmploymentRates(EmploymentRateQueryFilter criteria);
        Task<EmploymentRateResponseDto> UpdateEmploymentRateForUser(EmploymentRateChangeRequestDto request);
    }

    public class UserQuerySearch
    {
        public int? Id { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? EmployeeId { get; set; }
    }

    public class EmploymentRateQueryFilter
    {
        public int? UserId { get; set; }
        public decimal? Rate { get; set; }
    }
}