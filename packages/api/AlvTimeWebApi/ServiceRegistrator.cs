using AlvTime.Business;
using AlvTime.Business.Absence;
using AlvTime.Business.AccessTokens;
using AlvTime.Business.AssociatedTask;
using AlvTime.Business.CompensationRate;
using AlvTime.Business.Customers;
using AlvTime.Business.Economy;
using AlvTime.Business.Holidays;
using AlvTime.Business.HourRates;
using AlvTime.Business.Interfaces;
using AlvTime.Business.Payouts;
using AlvTime.Business.Projects;
using AlvTime.Business.Tasks;
using AlvTime.Business.TimeRegistration;
using AlvTime.Business.Users;
using AlvTime.Business.Utils;
using AlvTime.Persistence.Repositories;
using AlvTimeWebApi.Controllers.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlvTimeWebApi
{
    public static class ServiceRegistrator
    {
        public static void AddAlvtimeServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddTransient<IUserStorage, UserStorage>();
            services.AddHttpContextAccessor();
            services.AddScoped<RetrieveUsers>();
            services.AddScoped<UserCreator>();
            services.AddScoped<AlvHoursCalculator>();
            services.AddScoped<ITaskStorage, TaskStorage>();
            services.AddScoped<ITimeRegistrationStorage, TimeRegistrationStorage>();
            services.AddScoped<IHourRateStorage, HourRateStorage>();
            services.AddScoped<HourRateCreator>();
            services.AddScoped<IProjectStorage, ProjectStorage>();
            services.AddScoped<ProjectCreator>();
            services.AddScoped<ICustomerStorage, CustomerStorage>();
            services.AddScoped<CustomerCreator>();
            services.AddScoped<IEconomyStorage, EconomyStorage>();
            services.AddScoped<IAccessTokenStorage, AccessTokenStorage>();
            services.AddScoped<IAssociatedTaskStorage, AssociatedTaskStorage>();
            services.AddScoped<ICompensationRateStorage, CompensationRateStorage>();
            services.AddScoped<IRedDaysService, RedDaysService>();
            services.AddScoped<IAbsenceDaysService, AbsenceDaysService>();
            services.AddScoped<AccessTokenService>();
            services.AddScoped<IUserContext, UserContext>();
            services.AddScoped<IPayoutStorage, PayoutStorage>();
            services.AddScoped<PayoutService>();
            services.AddScoped<TaskUtils>();
            services.AddScoped<TimeRegistrationService>();
            services.AddScoped<IDbContextScope, DbContextScope>();
            services.AddScoped<TaskService>();
        }
    }
}
