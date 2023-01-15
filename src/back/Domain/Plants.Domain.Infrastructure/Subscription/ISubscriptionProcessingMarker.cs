namespace Plants.Domain.Infrastructure.Subscription;

public interface ISubscriptionProcessingNotificator
{ 
    void SubscribeToNotifications(AggregateDescription description);
    void UnsubscribeFromNotifications(AggregateDescription description);
    bool WasProcessed(AggregateDescription description);
}

internal interface ISubscriptionProcessingMarker
{
    void MarkSubscribersCount(AggregateDescription description, long subscriptionsCount);
    void MarkSubscriptionComplete(AggregateDescription description);
}