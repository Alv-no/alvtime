using AlvTime.Persistence.DataBaseModels;
using AlvTimeWebApi.Authentication;
using AlvTimeWebApi.Authorization;
using AlvTimeWebApi.ErrorHandling;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace AlvTimeWebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        private const string DevCorsPolicyName = "devCorsPolicyName";

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAlvtimeServices(Configuration);
            services.AddDbContext<AlvTime_dbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("AlvTime_db")), contextLifetime: ServiceLifetime.Scoped, optionsLifetime: ServiceLifetime.Scoped);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddAlvtimeAuthentication(Configuration);
            services.AddAlvtimeAuthorization();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Example API", Version = "v1" });
            });

            services.AddRazorPages();

            services.AddCors(options =>
            {
                options.AddPolicy(name: DevCorsPolicyName,
                    builder =>
                    {
                        builder.AllowAnyOrigin();
                        builder.AllowAnyMethod();
                        builder.AllowAnyHeader();
                    }
                );
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseHsts(); // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            } else
            {
                app.UseCors(DevCorsPolicyName);
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseErrorHandling(env);

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
