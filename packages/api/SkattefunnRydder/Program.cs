// See https://aka.ms/new-console-template for more information


using AlvTime.Business.Options;
using AlvTime.Persistence.DatabaseModels;
using AlvTime.Persistence.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SkattefunnRydder;



HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);


builder.Services.AddDbContext<AlvTime_dbContext>(
     options => options.UseSqlServer(""),
     contextLifetime: ServiceLifetime.Scoped, optionsLifetime: ServiceLifetime.Scoped);     


using IHost host = builder.Build();

//ExemplifyServiceLifetime(host.Services, "Lifetime 1");
//ExemplifyServiceLifetime(host.Services, "Lifetime 2");

var dbContextOptions = new DbContextOptions<AlvTime_dbContext>();

var alvTime_dbContext = new AlvTime_dbContext(dbContextOptions);
var timeRegistrationStorage = new TimeRegistrationStorage(alvTime_dbContext);
var s = new SkattefunnProgram(timeRegistrationStorage);
await s.RunSkattefunnProgram();


await host.RunAsync();




