using AlvTime.Business;
using AlvTime.Business.Tasks;
using AlvTime.Business.Users;
using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Controllers.Admin.Users.UserStorage;
using AlvTimeWebApi.Controllers.Tasks.TaskStorage;
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
        }
    }
}
