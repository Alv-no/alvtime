using AlvTime.Business.Events;
using System;

namespace AlvTime.Business.TimeRegistration.Events;

public record TimeEntryCreatedEvent(int Id, int UserId, DateTime Date)
    : IWebhookPayload
{
    public static string Topic => "time_entry.created";
    public static int Version => 1;
}
