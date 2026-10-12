using System;

namespace AlvTime.Business.Events;

public interface IWebhookPayload
{
    static abstract string Topic { get; }

    // Bump when the payload shape changes in a breaking way
    static abstract int Version { get; }
}

public record WebhookEvent<TData>(string EventId, DateTimeOffset Timestamp, TData Data)
    where TData : IWebhookPayload
{
    public string Topic => TData.Topic;
    public int Version => TData.Version;
}
