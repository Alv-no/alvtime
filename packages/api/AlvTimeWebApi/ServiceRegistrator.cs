using AlvTime.Business;
using AlvTime.Business.AbsenseDays;
using AlvTime.Business.AccessToken;
using AlvTime.Business.AccessToken.PersonalAccessToken;
using AlvTime.Business.AssociatedTask;
using AlvTime.Business.CompensationRate;
using AlvTime.Business.Customers;
using AlvTime.Business.Economy;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.Holidays;
using AlvTime.Business.HourRates;
using AlvTime.Business.Projects;
using AlvTime.Business.Tasks;
using AlvTime.Business.Tasks.Admin;
using AlvTime.Business.TimeEntries;
using AlvTime.Business.Users;
using AlvTime.Persistence.Repositories;
using AlvTimeWebApi.Controllers.Utils;
using AlvTimeWebApi.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlvTimeWebApi
{
    public static class ServiceRegistrator
    {
        public static void AddAlvtimeServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IPersonalAccessTokenStorage, PersonalAccessTokenRepository>();
            services.AddTransient<IUserStorage, UserStorage>();
            services.AddHttpContextAccessor();
            services.AddScoped<RetrieveUsers>();
            services.AddScoped<UserCreator>();
            services.AddScoped<AlvHoursCalculator>();
            services.AddScoped<ITaskStorage, TaskStorage>();
            services.AddScoped<FavoriteUpdater>();
            services.AddScoped<TaskCreator>();
            services.AddScoped<ITimeEntryStorage, TimeEntryStorage>();
            services.AddScoped<TimeEntryCreator>();
            services.AddScoped<IHourRateStorage, HourRateStorage>();
            services.AddScoped<HourRateCreator>();
            services.AddScoped<IProjectStorage, ProjectStorage>();
            services.AddScoped<ProjectCreator>();
            services.AddScoped<ICustomerStorage, CustomerStorage>();
            services.AddScoped<CustomerCreator>();
            services.AddScoped<IEconomyStorage, EconomyStorage>();
            services.AddScoped<IFlexhourStorage, FlexhourStorage>();
            services.AddScoped<IAbsenseDaysService, AbsenseDaysService>();
            services.AddScoped<IAccessTokenStorage, AccessTokenStorage>();
            services.AddScoped<IAssociatedTaskStorage, AssociatedTaskStorage>();
            services.AddScoped<ICompensationRateStorage, CompensationRateStorage>();
            services.AddScoped<IRedDaysService, RedDaysService>();
        }
    }
}
