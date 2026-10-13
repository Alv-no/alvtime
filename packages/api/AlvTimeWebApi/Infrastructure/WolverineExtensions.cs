using AlvTime.Business.Events;
using AlvTime.Business.TimeRegistration.Events;
using AlvTimeWebApi.BackgroundServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using Wolverine;
using Wolverine.EntityFrameworkCore;
using Wolverine.ErrorHandling;
using Wolverine.SqlServer;

namespace AlvTimeWebApi.Infrastructure
{
    public static class WolverineExtensions
    {
        public static void AddAlvtimeWolverine(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("AlvTime") ?? throw new InvalidOperationException("Connection string 'AlvTime' not found.");

            // Masstransit has gone commercial, goes for Wolverine instead
            services.AddWolverine(opts =>
            {
                opts.Policies.DisableConventionalLocalRouting();

                opts.PersistMessagesWithSqlServer(
                    connectionString,
                    "wolverine");

                opts.UseEntityFrameworkCoreTransactions();
                opts.Policies.AutoApplyTransactions();

                opts.Policies.UseDurableOutboxOnAllSendingEndpoints();
                // opts.Policies.UseDurableInboxOnAllListeners(); // Dont need inbox pattern for now

                // Explicitly register a handler (like AddConsumer<T>)
                opts.Discovery.IncludeType<WebhookOutboxProcessor>();

                // Conventional local routing is disabled, so route webhook events to a durable local queue explicitly
                opts.PublishMessage<WebhookEvent<TimeEntryCreatedEvent>>().ToLocalQueue("webhooks");
                opts.PublishMessage<WebhookEvent<TimeEntryUpdatedEvent>>().ToLocalQueue("webhooks");
                opts.LocalQueue("webhooks").UseDurableInbox();

                opts.OnException<System.Net.Http.HttpRequestException>()
                    .RetryWithCooldown(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(30))
                    .Then.ScheduleRetry(TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(30), TimeSpan.FromHours(2))
                    .Then.MoveToErrorQueue();
            });

        }

    }
}
