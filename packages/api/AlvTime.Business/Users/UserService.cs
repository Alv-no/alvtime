using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Utils;
using FluentValidation;

namespace AlvTime.Business.Users;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly ITimeRegistrationStorage _timeRegistrationStorage;

    public UserService(IUserRepository userRepository, ITimeRegistrationStorage timeRegistrationStorage)
    {
        _userRepository = userRepository;
        _timeRegistrationStorage = timeRegistrationStorage;
    }

    public async Task<IEnumerable<UserDto>> CreateUsers(IEnumerable<UserDto> users)
    {
        var response = new List<UserDto>();
        foreach (var user in users)
        {
            response.Add(await CreateUser(user));
        }

        return response;
    }

    private async Task<UserDto> CreateUser(UserDto user)
    {
        await CheckForDuplicateUserDetails(user);
        if (user.EndDate.HasValue && user.StartDate >= user.EndDate)
        {
            throw new ValidationException($"Sluttdato må være etter startdato for {user.Name}");
        }

        await _userRepository.AddUser(user);

        return await GetUser(user);
    }

    public async Task<IEnumerable<UserDto>> UpdateUsers(IEnumerable<UserDto> users)
    {
        var allUsers = (await _userRepository.GetUsers(new UserQuerySearch())).ToList();

        var response = new List<UserDto>();
        foreach (var user in users)
        {
            if (allUsers.Any(u => u.Id != user.Id && (u.EmployeeId == user.EmployeeId || u.Email == user.Email || u.Name == user.Name)))
            {
                throw new DuplicateNameException("En bruker har allerede blitt tildelt det ansattnummeret");
            }

            response.Add(await UpdateUser(user));
        }

        return response;
    }

    private async Task<UserDto> UpdateUser(UserDto user)
    {
        var allUsers = await _userRepository.GetUsers(new UserQuerySearch());
        if (allUsers.Any(u => u.Id != user.Id && (u.EmployeeId == user.EmployeeId || u.Email == user.Email || u.Name == user.Name)))
        {
            throw new DuplicateNameException("En bruker har allerede blitt tildelt det ansattnummeret");
        }

        await _userRepository.UpdateUser(user);

        return await GetUserById(user.Id);
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

    public async Task<List<UserDto>> GetUsers(UserQuerySearch criteria)
    {
        var users = (await _userRepository.GetUsers(criteria)).ToList();

        return users;
    }

    private async Task<UserDto> GetUser(UserDto userToFetch)
    {
        var user = (await _userRepository.GetUsersWithEmploymentRates(new UserQuerySearch
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

    public async Task<UserDto> GetUserById(int userId)
    {
        var user = (await _userRepository.GetUsersWithEmploymentRates(new UserQuerySearch
        {
            Id = userId
        })).FirstOrDefault();

        if (user == null)
        {
            throw new Exception($"User with id {userId} was not found");
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
    
    public async Task<IEnumerable<EmploymentRateResponseDto>> CreateEmploymentRatesForUser(IEnumerable<EmploymentRateDto> requests)
    {
        var response = new List<EmploymentRateResponseDto>();
        foreach (var request in requests)
        {
            response.Add(await CreateEmploymentRateForUser(request));
        }

        return response;
    }

    private async Task<EmploymentRateResponseDto> CreateEmploymentRateForUser(EmploymentRateDto request)
    {
        var existingEmploymentRates = await _userRepository.GetEmploymentRates(new EmploymentRateQueryFilter { UserId = request.UserId });
        await ValidateRateUpdate(request.FromDateInclusive, request.ToDateInclusive, request.UserId, existingEmploymentRates);

        return await _userRepository.CreateEmploymentRateForUser(request);
    }

    public async Task<IEnumerable<EmploymentRateResponseDto>> UpdateEmploymentRatesForUser(IEnumerable<EmploymentRateChangeRequestDto> requests)
    {
        var response = new List<EmploymentRateResponseDto>();

        foreach (var request in requests)
        {
            response.Add(await UpdateEmploymentRateForUser(request));
        }

        return response;
    }

    private async Task<EmploymentRateResponseDto> UpdateEmploymentRateForUser(EmploymentRateChangeRequestDto request)
    {
        var existingEmploymentRates = (await _userRepository.GetEmploymentRates(new EmploymentRateQueryFilter { UserId = request.UserId })).Where(rate => rate.Id != request.RateId);
        await ValidateRateUpdate(request.FromDateInclusive, request.ToDateInclusive, request.UserId, existingEmploymentRates);

        return await _userRepository.UpdateEmploymentRateForUser(request);
    }

    private async Task ValidateRateUpdate(DateTime fromDateInclusive, DateTime toDateInclusive, int userId, IEnumerable<EmploymentRateResponseDto> existingEmploymentRates)
    {
        var datesForExistingRates =
            existingEmploymentRates.SelectMany(e => DateUtils.EachDay(e.FromDateInclusive, e.ToDateInclusive));
        var datesForNewRate = DateUtils.EachDay(fromDateInclusive, toDateInclusive);
        if (datesForExistingRates.Intersect(datesForNewRate).Any())
        {
            throw new ValidationException("Brukeren har allerede en stillingsprosent på valgt dato.");
        }

        var userTimeEntriesInDateRange = (await _timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
        {
            FromDateInclusive = fromDateInclusive,
            ToDateInclusive = toDateInclusive,
            UserId = userId
        })).Where(te => te.Value > 0);

        if (userTimeEntriesInDateRange.Any())
        {
            throw new ValidationException("Brukeren har allerede førte timer på en dato stillingsprosenten gjelder for.");
        }
    }
}