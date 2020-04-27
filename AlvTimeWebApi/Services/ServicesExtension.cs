using AlvTime.Business;
using AlvTime.Business.AccessToken;
using AlvTime.Business.Customers;
using AlvTime.Business.Economy;
using AlvTime.Business.FlexiHours;
using AlvTime.Business.HourRates;
using AlvTime.Business.Projects;
using AlvTime.Business.Tasks;
using AlvTime.Business.Tasks.Admin;
using AlvTime.Business.TimeEntries;
using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Controllers.AccessToken.AccessTokenStorage;
using AlvTimeWebApi.Controllers.Admin.Customers.CustomerStorage;
using AlvTimeWebApi.Controllers.Admin.Economy.EconomyStorage;
using AlvTimeWebApi.Controllers.Admin.HourRates.HourRateStorage;
using AlvTimeWebApi.Controllers.Admin.Projects.ProjectStorage;
using AlvTimeWebApi.Controllers.Admin.Users.UserStorage;
using AlvTimeWebApi.Controllers.FlexiHours.FlexiHourStorage;
using AlvTimeWebApi.Controllers.Tasks.TaskStorage;
using AlvTimeWebApi.Controllers.TimeEntries.TimeEntryStorage;
using AlvTimeWebApi.HelperClasses;
using AlvTimeWebApi.Persistence.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlvTimeWebApi.Services
{
    public static class ServicesExtension
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
            services.AddScoped<IFlexiHourStorage, FlexiHourStorage>();
            services.AddScoped<IAccessTokenStorage, AccessTokenStorage>();
        }
    }
}
