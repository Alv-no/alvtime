using AlvTime.Business.Users;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using AlvTime.Business.AccessTokens;
using AlvTime.Business.Overtime;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;
using User = AlvTime.Persistence.DatabaseModels.User;

namespace AlvTime.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AlvTime_dbContext _context;

    public UserRepository(AlvTime_dbContext context)
    {
        _context = context;
    }

    public async Task AddUser(UserDto user)
    {
        var newUser = new User
        {
            Name = user.Name,
            Email = user.Email,
            StartDate = user.StartDate!.Value.Date,
            EndDate = user.EndDate,
            EmployeeId = user.EmployeeId ?? 0,
            Oid = user.Oid
        };

        _context.User.Add(newUser);
        await _context.SaveChangesAsync();

        _context.SalaryModelHistory.Add(new SalaryModelHistory
        {
            UserId = newUser.Id,
            SwitchDate = newUser.StartDate,
            PreviousModel = user.SalaryModel,
            NewModel = user.SalaryModel
        });
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserDto>> GetUsers(UserQuerySearch criteria)
    {
        var users = await _context.User.AsQueryable()
            .Filter(criteria)
            .Select(u => new UserDto
            {
                Email = u.Email,
                Id = u.Id,
                Name = u.Name,
                StartDate = u.StartDate,
                EndDate = u.EndDate,
                EmployeeId = u.EmployeeId,
                Oid = u.Oid,
            })
            .ToListAsync();
        await PopulateCurrentSalaryModel(users);
        return users;
    }

    public async Task<IEnumerable<UserDto>> GetUsersWithEmploymentRates(UserQuerySearch criteria)
    {
        var users = await _context.User.AsQueryable()
            .Filter(criteria)
            .Select(u => new UserDto
            {
                Email = u.Email,
                Id = u.Id,
                Name = u.Name,
                StartDate = u.StartDate,
                EndDate = u.EndDate,
                EmployeeId = u.EmployeeId,
                Oid = u.Oid,
                EmploymentRates = u.EmploymentRate.Select(er => new UserEmploymentRateDto
                {
                    Id = er.Id,
                    Rate = er.Rate,
                    FromDateInclusive = er.FromDate,
                    ToDateInclusive = er.ToDate
                })
            }).ToListAsync();
        await PopulateCurrentSalaryModel(users);
        return users;
    }

    private async Task PopulateCurrentSalaryModel(IList<UserDto> users)
    {
        if (users.Count == 0) return;
        var today = DateTime.Today;
        var userIds = users.Select(u => u.Id).ToList();
        var historyRows = await _context.SalaryModelHistory
            .Where(h => userIds.Contains(h.UserId) && h.SwitchDate.Date <= today)
            .OrderBy(h => h.SwitchDate)
            .ToListAsync();
        var historyByUser = historyRows
            .GroupBy(h => h.UserId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var user in users)
        {
            if (historyByUser.TryGetValue(user.Id, out var history) && history.Count > 0)
            {
                var entries = history
                    .Select(h => new SalaryModelHistoryEntry(h.SwitchDate, h.PreviousModel, h.NewModel))
                    .ToList();
                user.SalaryModel = SalaryModelHistoryHelper.GetModelAtDate(today, entries);
            }
            else
            {
                user.SalaryModel = SalaryModel.Static;
            }
        }
    }

    public async Task UpdateUser(UserDto userToBeUpdated)
    {
        var existingUser = await _context.User.FirstOrDefaultAsync(u => u.Id == userToBeUpdated.Id);

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

        if (userToBeUpdated.EmployeeId != null)
        {
            existingUser.EmployeeId = (int)userToBeUpdated.EmployeeId;
        }

        await _context.SaveChangesAsync();
    }

    public async Task<Business.Users.User> GetUserFromToken(Token token)
    {
        var databaseToken = await _context.AccessTokens.FirstOrDefaultAsync(x => x.Value == token.Value && x.ExpiryDate >= DateTime.UtcNow);

        if (databaseToken != null)
        {
            var databaseUser = await _context.User.FirstOrDefaultAsync(x => x.Id == databaseToken.UserId);

            return new Business.Users.User
            {
                Id = databaseUser.Id,
                Email = databaseUser.Email,
                Name = databaseUser.Name,
                EndDate = databaseUser.EndDate,
                Oid = databaseUser.Oid,
            };
        }

        return null;
    }

    public async Task<EmploymentRateResponseDto> CreateEmploymentRateForUser(EmploymentRateDto input)
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

        return new EmploymentRateResponseDto
        {
            Id = rate.Id,
            UserId = rate.UserId,
            Rate = rate.Rate,
            FromDateInclusive = rate.FromDate,
            ToDateInclusive = rate.ToDate
        };
    }

    public async Task<IEnumerable<EmploymentRateResponseDto>> GetEmploymentRates(EmploymentRateQueryFilter criteria)
    {
        var rates = await _context.EmploymentRate.AsQueryable()
            .Filter(criteria)
            .Select(er => new EmploymentRateResponseDto
            {
                Id = er.Id,
                UserId = er.UserId,
                Rate = er.Rate,
                FromDateInclusive = er.FromDate,
                ToDateInclusive = er.ToDate
            }).ToListAsync();
        return rates;
    }

    public async Task<EmploymentRateResponseDto> UpdateEmploymentRateForUser(EmploymentRateDto request)
    {
        var existingRate = await _context.EmploymentRate.FirstOrDefaultAsync(e => e.Id == request.RateId);
        if (existingRate == null)
        {
            throw new SqlNullValueException("Rate was not found");
        }

        existingRate.Rate = request.Rate;
        existingRate.FromDate = request.FromDateInclusive.Date;
        existingRate.ToDate = request.ToDateInclusive.Date;

        await _context.SaveChangesAsync();
        return new EmploymentRateResponseDto
        {
            Id = existingRate.Id,
            UserId = existingRate.UserId,
            Rate = existingRate.Rate,
            FromDateInclusive = existingRate.FromDate,
            ToDateInclusive = existingRate.ToDate
        };
    }

    public async Task<IEnumerable<SalaryModelHistoryEntry>> GetSalaryModelHistory(int userId)
    {
        return await _context.SalaryModelHistory
            .Where(h => h.UserId == userId)
            .OrderBy(h => h.SwitchDate)
            .Select(h => new SalaryModelHistoryEntry(h.SwitchDate, h.PreviousModel, h.NewModel))
            .ToListAsync();
    }

    public async Task AddSalaryModelHistory(int userId, SalaryModelHistoryEntry entry)
    {
        _context.SalaryModelHistory.Add(new SalaryModelHistory
        {
            UserId = userId,
            SwitchDate = entry.SwitchDate,
            PreviousModel = entry.PreviousModel,
            NewModel = entry.NewModel
        });
        await _context.SaveChangesAsync();
    }

    public async Task UpdateSalaryModelHistory(int userId, SalaryModelHistoryEntry entry)
    {
        var salaryModelHistoryEntry = await _context.SalaryModelHistory
            .FirstOrDefaultAsync(h => h.UserId == userId && h.SwitchDate == entry.SwitchDate);
        salaryModelHistoryEntry.PreviousModel = entry.PreviousModel;
        salaryModelHistoryEntry.NewModel = entry.NewModel;
        await _context.SaveChangesAsync();
    }

    public async Task DeleteSalaryModelHistoryAfter(int userId, DateTime threshold)
    {
        var pending = _context.SalaryModelHistory
            .Where(h => h.UserId == userId && h.SwitchDate > threshold);
        _context.SalaryModelHistory.RemoveRange(pending);
        await _context.SaveChangesAsync();
    }
}