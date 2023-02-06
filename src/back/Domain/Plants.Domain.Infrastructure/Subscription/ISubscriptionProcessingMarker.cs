namespace Plants.Domain.Infrastructure;

public interface ISubscriptionProcessingSubscription
{ 
    void SubscribeToNotifications(AggregateDescription description, string? notifyUsername);
    void UnsubscribeFromNotifications(AggregateDescription description);
    bool WasProcessed(AggregateDescription description);
}

internal interface ISubscriptionProcessingMarker
{
    void MarkSubscribersCount(AggregateDescription description, long subscriptionsCount);
    SubscriptionState? MarkSubscriptionComplete(AggregateDescription description);
}

internal record SubscriptionState(bool IsProcessed, string? NotifyUsername);