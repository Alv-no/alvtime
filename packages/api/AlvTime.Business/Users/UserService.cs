using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
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
        await CheckForDuplicateUserDetails(user);
        await _userRepository.AddUser(user);

        return await GetUser(user);
    }

    public async Task<UserResponseDto> UpdateUser(UserDto user)
    {
        var allUsers = await _userRepository.GetUsers(new UserQuerySearch());
        if (allUsers.Any(u => u.Id != user.Id && u.EmployeeId == user.EmployeeId))
        {
            throw new DuplicateNameException("En bruker har allerede blitt tildelt det ansattnummeret");
        }
        await _userRepository.UpdateUser(user);

        return await GetUserById(user);
    }

    private async Task CheckForDuplicateUserDetails(UserDto userToBeCreated)
    {
        var userWithEmail = (await _userRepository.GetUsers(new UserQuerySearch
        {
            Email = userToBeCreated.Email,
        })).FirstOrDefault();
        if (userWithEmail != null)
        {
            throw new DuplicateNameException("Bruker med gitt epost finnes allerede");
        }

        var userWithName = (await _userRepository.GetUsers(new UserQuerySearch
        {
            Name = userToBeCreated.Name,
        })).FirstOrDefault();
        if (userWithName != null)
        {
            throw new DuplicateNameException("Bruker med gitt navn finnes allerede");
        }

        var userWithEmployeeId = (await _userRepository.GetUsers(new UserQuerySearch
        {
            EmployeeId = userToBeCreated.EmployeeId,
        })).FirstOrDefault();
        if (userWithEmployeeId != null)
        {
            throw new DuplicateNameException("Bruker med gitt ansattnummer finnes allerede");
        }
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
        var rates = (await _userRepository.GetEmploymentRates(new EmploymentRateQueryFilter { UserId = userId }))
            .Where(er =>
                er.FromDateInclusive.Date <= timeEntryDate.Date && er.ToDateInclusive.Date >= timeEntryDate.Date)
            .ToList();

        if (rates.Count > 1)
        {
            throw new ValidationException("Bruker har mer enn 1 gyldig stillingsprosent for gitt dato");
        }

        return rates.Any() ? rates.First().Rate : 1;
    }
}