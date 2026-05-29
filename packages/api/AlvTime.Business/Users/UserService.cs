using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.Overtime;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Utils;

namespace AlvTime.Business.Users;

public class UserService(IUserRepository userRepository, ITimeRegistrationStorage timeRegistrationStorage, DateAlvTime dateAlvTime)
{
    public UserService(IUserRepository userRepository, ITimeRegistrationStorage timeRegistrationStorage)
        : this(userRepository, timeRegistrationStorage, new DateAlvTime()) { }


    public async Task<Result<UserDto>> CreateUser(UserDto user)
    {
        var errors = new List<Error>();
        await CheckForDuplicateUserDetails(user, errors);
        if (user.EndDate.HasValue && user.StartDate >= user.EndDate)
        {
            errors.Add(new Error(ErrorCodes.RequestInvalidProperty, "Sluttdato må være etter startdato"));
        }

        if (errors.Any())
        {
            return errors;
        }

        await userRepository.AddUser(user);

        return await GetUser(user);
    }

    public async Task<Result<UserDto>> UpdateUser(UserDto user)
    {
        var allUsers = await userRepository.GetUsers(new UserQuerySearch());
        
        if (allUsers.Any(u => u.Id != user.Id && 
                              (u.EmployeeId == user.EmployeeId || 
                               (u.Email == user.Email && (u.EndDate == null || u.EndDate > DateTime.Now)) ||
                               u.Name == user.Name)))
        {
            return new Error(ErrorCodes.EntityAlreadyExists, "En bruker har allerede blitt tildelt det ansattnummeret, eposten eller navnet.");
        }

        await userRepository.UpdateUser(user);
        return user;
    }

    private async Task CheckForDuplicateUserDetails(UserDto userToBeCreated, List<Error> errors)
    {
        var userWithEmail = (await userRepository.GetUsers(new UserQuerySearch
        {
            Email = userToBeCreated.Email,
        })).FirstOrDefault();
        if (userWithEmail != null && (userWithEmail.EndDate == null || userWithEmail.EndDate > DateTime.UtcNow))
        {
            errors.Add(new Error(ErrorCodes.EntityAlreadyExists, "Aktiv bruker med gitt epost finnes allerede."));
        }

        var userWithName = (await userRepository.GetUsers(new UserQuerySearch
        {
            Name = userToBeCreated.Name,
        })).FirstOrDefault();
        if (userWithName != null)
        {
            errors.Add(new Error(ErrorCodes.EntityAlreadyExists, "Bruker med gitt navn finnes allerede."));
        }

        var userWithEmployeeId = (await userRepository.GetUsers(new UserQuerySearch
        {
            EmployeeId = userToBeCreated.EmployeeId,
        })).FirstOrDefault();
        if (userWithEmployeeId != null)
        {
            errors.Add(new Error(ErrorCodes.EntityAlreadyExists, "Bruker med gitt ansattnummer finnes allerede."));
        }
    }

    public async Task<Result<List<UserDto>>> GetUsers(UserQuerySearch criteria)
    {
        return (await userRepository.GetUsersWithEmploymentRates(criteria)).ToList();
    }

    private async Task<UserDto> GetUser(UserDto userToFetch)
    {
        var user = (await userRepository.GetUsersWithEmploymentRates(new UserQuerySearch
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

    public async Task<Result<UserDto>> UpdateSalaryModel(int userId, SalaryModel newSalaryModel)
    {
        var user = await GetUserById(userId);
        if (user is null)
            return new Error(ErrorCodes.MissingEntity, "Bruker ble ikke funnet.");

        var today = dateAlvTime.Now.Date;
        var switchDate = new DateTime(today.Year, 6, 1);

        if (today.Month >= 7)
            return new Error(ErrorCodes.InvalidAction, "Lønnsmodell kan bare endres frem til 30. juni.");

        var history = (await userRepository.GetSalaryModelHistory(userId)).ToList();

        if (today >= switchDate
            && history.Any(h => h.SwitchDate.Year == today.Year && h.PreviousModel != h.NewModel))
            return new Error(ErrorCodes.InvalidAction, "Lønnsmodellen er allerede endret i år.");

        var previousModel = SalaryModelHistoryHelper.GetModelAtDate(
            switchDate.AddDays(-1),
            history.Where(h => h.SwitchDate.Date < switchDate).ToList());

        if (newSalaryModel == previousModel)
            return new Error(ErrorCodes.InvalidAction, "Lønnsmodellen er allerede satt til den valgte verdien.");
        
        if (user.StartDate > dateAlvTime.Now)
        {
            switchDate = user.StartDate.Value.Date;
            previousModel = newSalaryModel;
        }

        await userRepository.DeleteSalaryModelHistoryAfter(userId, switchDate.AddDays(-1));
        await userRepository.AddSalaryModelHistory(userId, new SalaryModelHistoryEntry(switchDate, previousModel, newSalaryModel));
        return await GetUserById(userId);
    }

    public async Task<IEnumerable<SalaryModelHistoryEntry>> GetSalaryModelHistory(int userId)
    {
        return await userRepository.GetSalaryModelHistory(userId);
    }

    public async Task<Result<UserDto>> CancelPendingSalaryModelChange(int userId)
    {
        var user = await GetUserById(userId);
        if (user is null)
            return new Error(ErrorCodes.MissingEntity, "Bruker ble ikke funnet.");

        if (user.StartDate > dateAlvTime.Now)
            return new Error(ErrorCodes.InvalidAction, "Kan ikke kansellere endring for en ansatt som ikke har startet enda.");

        var today = dateAlvTime.Now.Date;
        var history = (await userRepository.GetSalaryModelHistory(userId)).ToList();
        if (!history.Any(h => h.SwitchDate.Date > today))
            return new Error(ErrorCodes.InvalidAction, "Det finnes ingen planlagt endring å avbryte.");

        await userRepository.DeleteSalaryModelHistoryAfter(userId, today);
        return await GetUserById(userId);
    }

    public async Task<UserDto> GetUserById(int userId)
    {
        var user = (await userRepository.GetUsersWithEmploymentRates(new UserQuerySearch
        {
            Id = userId
        })).FirstOrDefault();
        if (user is null) return null;

        var today = dateAlvTime.Now.Date;
        var history = (await userRepository.GetSalaryModelHistory(userId)).ToList();
        var pending = history.FirstOrDefault(h => h.SwitchDate.Date > today);
        var displayedHistory = history
            .Where(h => h.SwitchDate.Date <= today && h.PreviousModel != h.NewModel)
            .ToList();

        user.PendingSalaryModelChange = pending is null ? null : new PendingSalaryModelChangeDto(pending.SwitchDate, pending.NewModel);
        user.SalaryModelHistory = displayedHistory;
        return user;
    }

    public async Task<Result<decimal>> GetCurrentEmploymentRateForUser(int userId, DateTime timeEntryDate)
    {
        var rates = (await userRepository.GetEmploymentRates(new EmploymentRateQueryFilter { UserId = userId }))
            .Where(er =>
                er.FromDateInclusive.Date <= timeEntryDate.Date && er.ToDateInclusive.Date >= timeEntryDate.Date)
            .ToList();

        if (rates.Count > 1)
        {
            return new Error(ErrorCodes.InvalidAction, "Bruker har mer enn 1 gyldig stillingsprosent for gitt dato");
        }

        return rates.Any() ? rates.First().Rate : 1;
    }

    public async Task<Result<EmploymentRateResponseDto>> CreateEmploymentRateForUser(EmploymentRateDto request)
    {
        var existingEmploymentRates = await userRepository.GetEmploymentRates(new EmploymentRateQueryFilter { UserId = request.UserId });
        var errors = new List<Error>();
        await ValidateRateUpdate(request.FromDateInclusive, request.ToDateInclusive, request.UserId, existingEmploymentRates, errors);
        if (errors.Any())
        {
            return errors;
        }

        return await userRepository.CreateEmploymentRateForUser(request);
    }

    public async Task<Result<EmploymentRateResponseDto>> UpdateEmploymentRateForUser(EmploymentRateDto request)
    {
        var existingEmploymentRates = (await userRepository.GetEmploymentRates(new EmploymentRateQueryFilter { UserId = request.UserId })).Where(rate => rate.Id != request.RateId);
        var errors = new List<Error>();
        await ValidateRateUpdate(request.FromDateInclusive, request.ToDateInclusive, request.UserId, existingEmploymentRates, errors, request.RateId);
        if (errors.Any())
        {
            return errors;
        }
        
        return await userRepository.UpdateEmploymentRateForUser(request);
    }

    private async Task ValidateRateUpdate(DateTime fromDateInclusive, DateTime toDateInclusive, int userId, IEnumerable<EmploymentRateResponseDto> existingEmploymentRates, List<Error> errors, int? rateToUpdateId = null)
    {
        var datesForExistingRates =
            existingEmploymentRates.SelectMany(e => DateUtils.EachDay(e.FromDateInclusive, e.ToDateInclusive));
        var datesForNewRate = DateUtils.EachDay(fromDateInclusive, toDateInclusive);
        if (datesForExistingRates.Intersect(datesForNewRate).Any())
        {
            errors.Add(new Error(ErrorCodes.EntityAlreadyExists, "Brukeren har allerede en stillingsprosent på valgt dato."));
        }

        var userTimeEntriesInDateRange = (await timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
        {
            FromDateInclusive = fromDateInclusive,
            ToDateInclusive = toDateInclusive,
            UserId = userId
        })).Where(te => te.Value > 0);

        if (userTimeEntriesInDateRange.Any())
        {
            errors.Add(new Error(ErrorCodes.InvalidAction, "Endringen vil påvirke eksisterende timer."));
        }

        if (rateToUpdateId != null)
        {
            var existingRate = (await userRepository.GetEmploymentRates(new EmploymentRateQueryFilter { UserId = userId })).First(rate => rate.Id == rateToUpdateId);
            
            var userEntriesInDateRange = (await timeRegistrationStorage.GetTimeEntries(new TimeEntryQuerySearch
            {
                FromDateInclusive = existingRate.FromDateInclusive,
                ToDateInclusive = existingRate.ToDateInclusive,
                UserId = userId
            })).Where(te => te.Value > 0);
            
            if (userEntriesInDateRange.Any())
            {
                errors.Add(new Error(ErrorCodes.InvalidAction, "Endringen vil påvirke eksisterende timer."));
            }
        }
    }
}