using System;
using System.Linq;
using AlvTime.Business.Users;
using AlvTimeWebApi.Requests;
using AlvTimeWebApi.Responses;
using AlvTimeWebApi.Responses.Admin;
using AlvTimeWebApi.Utils;

namespace AlvTimeWebApi.Controllers.Utils;

public static class UserMapper
{
    public static User MapUserDtoToBusinessUser(UserDto dbUser)
    {
        return new User { Id = dbUser.Id, Email = dbUser.Email, Name = dbUser.Name, StartDate = dbUser.StartDate!.Value };
    }
    
    public static UserAdminResponse MapToUserResponse(this UserDto user)
    {
        return new UserAdminResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            StartDate = user.StartDate?.ToDateOnly(),
            EndDate = user.EndDate?.ToDateOnly(),
            EmployeeId = user.EmployeeId,
            EmploymentRates = user.EmploymentRates?.Select(rate => new UserEmploymentRateAdminResponse
            {
                Id = rate.Id,
                RatePercentage = decimal.Parse((rate.Rate * 100).ToString("0")),
                FromDateInclusive = rate.FromDateInclusive.ToDateOnly(),
                ToDateInclusive = rate.ToDateInclusive.ToDateOnly()
            })
        };
    }

    public static UserDto MapToUserDto(this UserCreateRequest user)
    {
        return new UserDto
        {
            Name = user.Name,
            Email = user.Email,
            StartDate = user.StartDate,
            EndDate = user.EndDate,
            EmployeeId = user.EmployeeId
        };
    }

    public static UserDto MapToUserDto(this UserUpdateRequest user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            StartDate = user.StartDate,
            EndDate = user.EndDate,
            EmployeeId = user.EmployeeId
        };
    }
    
    public static EmploymentRateDto MapToEmploymentRateDto(this EmploymentRateCreationRequest request, int userId)
    {
        return new EmploymentRateDto
        {
            UserId = userId,
            Rate = request.RatePercentage / 100M,
            ToDateInclusive = request.ToDateInclusive.Date,
            FromDateInclusive = request.FromDateInclusive.Date
        };
    }
    
    public static EmploymentRateResponse MapToEmploymentRateResponse(this EmploymentRateResponseDto rate)
    {
        return new EmploymentRateResponse
        {
            Id = rate.Id,
            UserId = rate.UserId,
            RatePercentage = decimal.Parse((rate.Rate * 100M).ToString("0")),
            FromDateInclusive = rate.FromDateInclusive.ToDateOnly(),
            ToDateInclusive = rate.ToDateInclusive.ToDateOnly()
        };
    }
    
    public static EmploymentRateChangeRequestDto MapToEmploymentRateChangeRequestDto(this EmploymentRateChangeRequest request, int userId)
    {
        return new EmploymentRateChangeRequestDto
        {
            UserId = userId,
            Rate = request.RatePercentage / 100M,
            ToDateInclusive = request.ToDateInclusive.Date,
            FromDateInclusive = request.FromDateInclusive.Date,
            RateId = request.Id
        };
    }
}