using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AlvTime.Business.Users;

public class UserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserResponseDto> CreateUser(UserDto user)
    {
        var userAlreadyExists = await UserAlreadyExists(user);
        if (!userAlreadyExists)
        {
            await _userRepository.AddUser(user);
        }

        return await GetUser(user);
    }

    public async Task<UserResponseDto> UpdateUser(UserDto user)
    {
        await _userRepository.UpdateUser(user);

        return await GetUserById(user);
    }

    private async Task<bool> UserAlreadyExists(UserDto userToBeCreated)
    {
        var user = (await _userRepository.GetUsers(new UserQuerySearch
        {
            Email = userToBeCreated.Email,
            Name = userToBeCreated.Name,
        })).FirstOrDefault();

        return user != null;
    }

    private async Task<UserResponseDto> GetUser(UserDto userToFetch)
    {
        var user = (await _userRepository.GetUsers(new UserQuerySearch
        {
            Email = userToFetch.Email,
            Name = userToFetch.Name,
        })).FirstOrDefault();

        if (user == null)
        {
            throw new Exception($"User with name {userToFetch.Name} was not found");
        }
            
        return user;
    }

    private async Task<UserResponseDto> GetUserById(UserDto userToFetch)
    {
        var user = (await _userRepository.GetUsers(new UserQuerySearch
        {
            Id = userToFetch.Id
        })).FirstOrDefault();
        
        if (user == null)
        {
            throw new Exception($"User with name {userToFetch.Name} was not found");
        }
        
        return user;
    }

    public async Task<decimal> GetCurrentEmploymentRateForUser(int userId, DateTime timeEntryDate)
    {
        var rates = (await _userRepository.GetEmploymentRates(new EmploymentRateQueryFilter { UserId = userId })).OrderByDescending(er => er.ToDateInclusive).ToList();

        if (rates.Any() && rates.First().FromDateInclusive.Date <= timeEntryDate.Date && rates.First().ToDateInclusive.Date >= timeEntryDate.Date)
        {
            return rates.First().Rate;
        }

        return 1;
    }
}