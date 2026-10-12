using AlvTime.Business.Events;
using AlvTime.Business.Options;
using AlvTime.Business.TimeRegistration.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AlvTimeWebApi.BackgroundServices
{
    // Wolverine handler for webhook events from the outbox, pushes the messages to Outpost which handle the webhook magic
    public class WebhookOutboxProcessor
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptionsMonitor<OutpostOptions> _outpostOptions;
        private readonly ILogger<WebhookOutboxProcessor> _logger;

        public WebhookOutboxProcessor(IHttpClientFactory httpClientFactory, IOptionsMonitor<OutpostOptions> outpostOptions, ILogger<WebhookOutboxProcessor> logger)
        {
            _httpClientFactory = httpClientFactory;
            _outpostOptions = outpostOptions;
            _logger = logger;
        }

        public Task Handle(WebhookEvent<TimeEntryCreatedEvent> evt, CancellationToken ct) => Send(evt, ct);

        public Task Handle(WebhookEvent<TimeEntryUpdatedEvent> evt, CancellationToken ct) => Send(evt, ct);

        private async Task Send<TData>(WebhookEvent<TData> evt, CancellationToken ct) where TData : IWebhookPayload
        {
            // No apiKey configured → webhooks are disabled
            if (string.IsNullOrEmpty(_outpostOptions.CurrentValue.ApiKey))
            {
                _logger.LogWarning("Outpost:ApiKey not configured, skipping webhook {Id}", evt.EventId);
                return;
            }

            var client = _httpClientFactory.CreateClient("WebhookOutpost");

            var response = await client.PostAsJsonAsync("publish", new
            {
                id = evt.EventId,
                tenant_id = _outpostOptions.CurrentValue.TenantId,
                topic = evt.Topic,
                time = evt.Timestamp,
                data = evt.Data,
                metadata = new { version = evt.Version.ToString() },
                eligible_for_retry = true
            }, ct);
            response.EnsureSuccessStatusCode(); // throw → retry policy kicks in

            _logger.LogInformation("Delivered webhook {Id}", evt.EventId);
        }
    }
}
