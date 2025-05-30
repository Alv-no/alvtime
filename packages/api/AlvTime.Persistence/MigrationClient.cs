using System;
using System.Collections.Generic;
using System.Linq;
using AlvTime.Persistence.DatabaseModels;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace AlvTime.Persistence;

public static class MigrationClient
{
    public static async Task RunMigrations(string connectionString, bool shouldSeed)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AlvTime_dbContext>()
            .UseSqlServer(connectionString);
        await using var context = new AlvTime_dbContext(optionsBuilder.Options);
        Console.WriteLine("Running migrations...");
        try
        {
            await context.Database.MigrateAsync();
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Migrations completed successfully");

            if (shouldSeed)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("Seeding database...");
                await SeedDatabase(context);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Seeding completed successfully");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Not a seeding environment. Skipping seeding");
            }
        }
        catch (Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Migrations failed");
            Console.WriteLine(e);
            throw;
        }
    }

    private static async Task SeedDatabase(AlvTime_dbContext context)
    {
        await SeedCustomers(context);
        await SeedProjects(context);
        await SeedUsers(context);
        await SeedTasks(context);
        await SeedTaskFavorites(context);
        await SeedHourRates(context);
        await SeedAccessTokens(context);
        await SeedCompensationRates(context);
    }

    private static async Task SeedCompensationRates(AlvTime_dbContext context)
    {
        var compensationRates = await context.CompensationRate.ToListAsync();
        if (!compensationRates.Any())
        {
            var tasks = await context.Task.ToListAsync();
            var newCompensationRates = new[] { 1.5M, 1.5M, 1.5M, 1.5M, 0.5M, 0.5M, 0.5M };

            for (var i = 0; i < tasks.Count; i++)
            {
                await context.CompensationRate.AddAsync(new CompensationRate
                {
                    FromDate = new DateTime(2019, 01, 01),
                    Value = newCompensationRates[i],
                    TaskId = tasks[i].Id
                });
            }

            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedAccessTokens(AlvTime_dbContext context)
    {
        var accessTokens = await context.AccessTokens.ToListAsync();
        if (!accessTokens.Any())
        {
            var significantUser = context.User.First(u => u.Email == "ahre-ketil.lillehagen@alvno.onmicrosoft.com");
            await context.AccessTokens.AddAsync(new AccessTokens
            {
                UserId = significantUser.Id,
                FriendlyName = "TestToken",
                ExpiryDate = new DateTime(2200, 01, 01),
                Value = "5801gj90-jf39-5j30-fjk3-480fj39kl409"
            });
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedHourRates(AlvTime_dbContext context)
    {
        var hourRates = await context.HourRate.ToListAsync();
        if (!hourRates.Any())
        {
            var rates = new[] { 1000, 800, 700, 600, 0, 0, 0 };
            var tasks = await context.Task.ToListAsync();
            for (var i = 0; i < tasks.Count; i++)
            {
                await context.AddAsync(new HourRate
                {
                    FromDate = new DateTime(2018, 12, 12),
                    Rate = rates[i],
                    TaskId = tasks[i].Id,
                });
            }

            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedTaskFavorites(AlvTime_dbContext context)
    {
        var favorites = await context.TaskFavorites.ToListAsync();
        if (!favorites.Any())
        {
            var users = await context.User.ToListAsync();
            foreach (var user in users)
            {
                var newFavorites = new List<TaskFavorites>
                {
                    new()
                    {
                        UserId = user.Id,
                        Task = context.Task.First(t => t.Name == "Seniorutvikler"),
                    },
                    new()
                    {
                        UserId = user.Id,
                        Task = context.Task.First(t => t.Name == "Sikkerhetstester"),
                    },
                    new()
                    {
                        UserId = user.Id,
                        Task = context.Task.First(t => t.Name == "AlvTimeUtvikling"),
                    }
                };
                await context.TaskFavorites.AddRangeAsync(newFavorites);
                await context.SaveChangesAsync();
            }
        }
    }

    private static async Task SeedTasks(AlvTime_dbContext context)
    {
        var tasks = await context.Task.ToListAsync();
        if (!tasks.Any())
        {
            var newTasks = new List<AlvTime.Persistence.DatabaseModels.Task>
            {
                new()
                {
                    Name = "Testleder",
                    Description = "",
                    ProjectNavigation = context.Project.First(p => p.Name == "MatAppen"),
                    Locked = false,
                    Favorite = false,
                    Imposed = false,
                },
                new()
                {
                    Name = "Pålagt overtid",
                    Description = "",
                    ProjectNavigation = context.Project.First(p => p.Name == "Sklier"),
                    Locked = false,
                    Favorite = false,
                    Imposed = true,
                },
                new()
                {
                    Name = "Seniorutvikler",
                    Description = "",
                    ProjectNavigation = context.Project.First(p => p.Name == "Luksussmellen"),
                    Locked = false,
                    Favorite = false,
                    Imposed = false,
                },
                new()
                {
                    Name = "Sikkerhetstester",
                    Description = "",
                    ProjectNavigation = context.Project.First(p => p.Name == "Luksussmellen"),
                    Locked = false,
                    Favorite = false,
                    Imposed = false,
                },
                new()
                {
                    Name = "AlvTimeUtvikling",
                    Description = "",
                    ProjectNavigation = context.Project.First(p => p.Name == "Alv"),
                    Locked = false,
                    Favorite = false,
                    Imposed = false,
                },
                new()
                {
                    Name = "Ferie",
                    Description = "",
                    ProjectNavigation = context.Project.First(p => p.Name == "Fravær"),
                    Locked = false,
                    Favorite = false,
                    Imposed = false,
                },
                new()
                {
                    Name = "Avspasering",
                    Description = "",
                    ProjectNavigation = context.Project.First(p => p.Name == "Fravær"),
                    Locked = false,
                    Favorite = false,
                    Imposed = false,
                }
            };
            await context.Task.AddRangeAsync(newTasks);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedUsers(AlvTime_dbContext context)
    {
        var users = await context.User.ToListAsync();
        if (!users.Any())
        {
            var newUsers = new List<User>
            {
                new()
                {
                    Name = "Test Testesen",
                    Email = "testesen@alv.no",
                    StartDate = new DateTime(2019, 08, 01),
                    EndDate = null,
                    EmployeeId = 1,
                    Oid = "12345678-1234-1234-1234-123456789012"
                },
                new()
                {
                    Name = "Ansatt to",
                    Email = "ansatto@alv.no",
                    StartDate = new DateTime(2019, 09, 01),
                    EndDate = null,
                    EmployeeId = 2,
                    Oid = "23456789-2345-2345-2345-234567890123"
                },
                new()
                {
                    Name = "Ahre Ketil Lillehagen",
                    Email = "ahre-ketil.lillehagen@alvno.onmicrosoft.com",
                    StartDate = new DateTime(2020, 11, 01),
                    EndDate = null,
                    EmployeeId = 3,
                    Oid = "34567890-3456-3456-3456-345678901234",
                    EmploymentRate = new List<EmploymentRate>
                    {
                        new()
                        {
                            Rate = 0.5M,
                            FromDate = new DateTime(2022, 01, 01),
                            ToDate = new DateTime(2022, 03, 01),
                        },
                        new()
                        {
                            Rate = 0.7M,
                            FromDate = new DateTime(2023, 01, 01),
                            ToDate = new DateTime(2024, 03, 01),
                        }
                    }
                }
            };
            await context.User.AddRangeAsync(newUsers);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedProjects(AlvTime_dbContext context)
    {
        var projects = await context.Project.ToListAsync();
        if (!projects.Any())
        {
            var newProjects = new List<Project>
            {
                new()
                {
                    Name = "MatAppen",
                    CustomerNavigation = context.Customer.First(c => c.Name == "SuperMat")
                },
                new()
                {
                    Name = "Sklier",
                    CustomerNavigation = context.Customer.First(c => c.Name == "Rutsjebaner AS")
                },
                new()
                {
                    Name = "Luksussmellen",
                    CustomerNavigation = context.Customer.First(c => c.Name == "Film og TV")
                },
                new()
                {
                    Name = "Alv",
                    CustomerNavigation = context.Customer.First(c => c.Name == "Alv")
                },
                new()
                {
                    Name = "Fravær",
                    CustomerNavigation = context.Customer.First(c => c.Name == "Alv")
                }
            };
            await context.Project.AddRangeAsync(newProjects);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedCustomers(AlvTime_dbContext context)
    {
        var customers = await context.Customer.ToListAsync();
        if (!customers.Any())
        {
            var newCustomers = new List<Customer>
            {
                new()
                {
                    Name = "SuperMat",
                    InvoiceAddress = "Alvvegen 34",
                    ContactPerson = "Supermann",
                    ContactEmail = "supermann@supermat.no",
                    ContactPhone = "81549300",
                    OrgNr = "738927593"
                },
                new()
                {
                    Name = "Rutsjebaner AS",
                    InvoiceAddress = "Alvvegen 21",
                    ContactPerson = "Willy",
                    ContactEmail = "willy@rutsjebaner.no",
                    ContactPhone = "53153162",
                    OrgNr = "893275893"
                },
                new()
                {
                    Name = "Film og TV",
                    InvoiceAddress = "Alvvegen 8",
                    ContactPerson = "Halvstrøm",
                    ContactEmail = "halvstrom@filmtv.no",
                    ContactPhone = "64136423",
                    OrgNr = "09532957"
                },
                new()
                {
                    Name = "Alv",
                    InvoiceAddress = "Alvvegen 69",
                    ContactPerson = "Sjefen",
                    ContactEmail = "sjefen@alv.no",
                    ContactPhone = "53178685",
                    OrgNr = "217983742"
                }
            };
            await context.Customer.AddRangeAsync(newCustomers);
            await context.SaveChangesAsync();
        }
    }
}