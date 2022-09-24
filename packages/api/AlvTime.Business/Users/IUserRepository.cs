using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AlvTime.Business.Models;

namespace AlvTime.Business.Users
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserResponseDto>> GetUsers(UserQuerySearch criteria);
        void AddUser(UserDto user);
        void UpdateUser(UserDto user);
        Task<User> GetUserFromToken(Token token);
        Task<EmploymentRateResponse> CreateEmploymentRateForUser(EmploymentRateDto input);
        Task<IEnumerable<EmploymentRateResponse>> GetEmploymentRates(EmploymentRateQueryFilter criteria);
        Task<EmploymentRateResponse> UpdateEmploymentRateForUser(EmploymentRateChangeRequest request);
    }

    public class UserQuerySearch
    {
        public int? Id { get; set; }
        public string Name { get; set; }

        public string Email { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class EmploymentRateQueryFilter
    {
        public int? UserId { get; set; }
        public decimal? Rate { get; set; }
    }
}