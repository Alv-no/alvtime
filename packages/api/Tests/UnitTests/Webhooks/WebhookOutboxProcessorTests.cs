using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using AlvTime.Business.Events;
using AlvTime.Business.Options;
using AlvTime.Business.TimeRegistration.Events;
using AlvTimeWebApi.BackgroundServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Tests.UnitTests.Webhooks;

public class WebhookOutboxProcessorTests
{
    [Fact]
    public async Task Handle_TimeEntryCreated_PostsOutpostPublishRequest()
    {
        var handler = new CapturingHandler();
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://outpost.test/2025-07-01/") };
        var factory = Mock.Of<IHttpClientFactory>(f => f.CreateClient("WebhookOutpost") == client);
        var options = Mock.Of<IOptionsMonitor<OutpostOptions>>(o => o.CurrentValue == new OutpostOptions { TenantId = "test-tenant", ApiKey = "secret" });
        var processor = new WebhookOutboxProcessor(factory, options, NullLogger<WebhookOutboxProcessor>.Instance);
        var timestamp = new DateTimeOffset(2026, 10, 8, 12, 0, 0, TimeSpan.Zero);
        var evt = new WebhookEvent<TimeEntryCreatedEvent>("evt-1", timestamp,
            new TimeEntryCreatedEvent(42, 4, new DateTime(2026, 10, 8)));

        await processor.Handle(evt, CancellationToken.None);

        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal("https://outpost.test/2025-07-01/publish", handler.Request.RequestUri!.ToString());
        var body = JsonDocument.Parse(handler.Body!).RootElement;
        Assert.Equal("evt-1", body.GetProperty("id").GetString());
        Assert.Equal("test-tenant", body.GetProperty("tenant_id").GetString());
        Assert.Equal("time_entry.created", body.GetProperty("topic").GetString());
        Assert.Equal(timestamp, body.GetProperty("time").GetDateTimeOffset());
        Assert.True(body.GetProperty("eligible_for_retry").GetBoolean());
        Assert.Equal("1", body.GetProperty("metadata").GetProperty("version").GetString());
        var data = body.GetProperty("data");
        Assert.Equal(["id", "userId", "date"], data.EnumerateObject().Select(p => p.Name));
        Assert.Equal(42, data.GetProperty("id").GetInt32());
        Assert.Equal(4, data.GetProperty("userId").GetInt32());
        Assert.Equal(new DateTime(2026, 10, 8), data.GetProperty("date").GetDateTime());
    }

    [Fact]
    public async Task Handle_WithoutClientSecret_DoesNotSendAnything()
    {
        var handler = new CapturingHandler();
        var client = new HttpClient(handler) { BaseAddress = new Uri("https://outpost.test/2025-07-01/") };
        var factory = Mock.Of<IHttpClientFactory>(f => f.CreateClient("WebhookOutpost") == client);
        var options = Mock.Of<IOptionsMonitor<OutpostOptions>>(o => o.CurrentValue == new OutpostOptions { TenantId = "test-tenant" });
        var processor = new WebhookOutboxProcessor(factory, options, NullLogger<WebhookOutboxProcessor>.Instance);
        var evt = new WebhookEvent<TimeEntryCreatedEvent>("evt-1", DateTimeOffset.UtcNow,
            new TimeEntryCreatedEvent(42, 4, new DateTime(2026, 10, 8)));

        await processor.Handle(evt, CancellationToken.None);

        Assert.Null(handler.Request);
    }

    [Fact]
    public async Task Handle_WithoutClientSecret_LogsWarning()
    {
        var client = new HttpClient(new CapturingHandler()) { BaseAddress = new Uri("https://outpost.test/2025-07-01/") };
        var factory = Mock.Of<IHttpClientFactory>(f => f.CreateClient("WebhookOutpost") == client);
        var options = Mock.Of<IOptionsMonitor<OutpostOptions>>(o => o.CurrentValue == new OutpostOptions { TenantId = "test-tenant" });
        var logger = new CapturingLogger();
        var processor = new WebhookOutboxProcessor(factory, options, logger);
        var evt = new WebhookEvent<TimeEntryCreatedEvent>("evt-1", DateTimeOffset.UtcNow,
            new TimeEntryCreatedEvent(42, 4, new DateTime(2026, 10, 8)));

        await processor.Handle(evt, CancellationToken.None);

        Assert.Contains(LogLevel.Warning, logger.Levels);
    }

    private class CapturingLogger : ILogger<WebhookOutboxProcessor>
    {
        public List<LogLevel> Levels { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => Levels.Add(logLevel);
    }

    private class CapturingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        public string? Body { get; private set; }

        protected override async System.Threading.Tasks.Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            Request = request;
            Body = await request.Content!.ReadAsStringAsync(ct);
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }
    }
}
