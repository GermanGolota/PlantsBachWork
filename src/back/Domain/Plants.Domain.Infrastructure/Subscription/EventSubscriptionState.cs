using System.Collections.Concurrent;

namespace Plants.Domain.Infrastructure.Subscription;

internal class EventSubscriptionState : ISubscriptionProcessingMarker, ISubscriptionProcessingNotificator, IEventSubscriptionState
{
    #region Processing
    private class SubscriptionProcessingState
    {
        private long _subscriptionToProcess = 0;
        private long _subscriptionProcessed = 0;
        private bool _isNew = true;

        public void MarkProcessed()
        {
            _subscriptionProcessed++;
            _isNew = false;
        }

        public void MarkSubscriptions(long count)
        {
            _subscriptionToProcess += count;
            _isNew = false;
        }

        public bool WasProcessed() =>
            _isNew is false && _subscriptionProcessed == _subscriptionToProcess;
    }

    private ConcurrentDictionary<string, SubscriptionProcessingState> _processingStates = new();

    public void SubscribeToNotifications(AggregateDescription description) =>
        GetProcessing(description, shouldCreate: true);

    public bool WasProcessed(AggregateDescription description) =>
        GetProcessing(description)?.WasProcessed() ?? false;

    public void UnsubscribeFromNotifications(AggregateDescription description) =>
        _processingStates.RemoveWithRetry(GetKey(description));

    public void MarkSubscriptionComplete(AggregateDescription description) =>
          GetProcessing(description)?.MarkProcessed();

    public void MarkSubscribersCount(AggregateDescription description, long subscriptionsCount) =>
        GetProcessing(description)?.MarkSubscriptions(subscriptionsCount);

    private SubscriptionProcessingState? GetProcessing(AggregateDescription description, bool shouldCreate = false)
    {
        SubscriptionProcessingState? result;

        var key = GetKey(description);
        if (shouldCreate is false)
        {
            if (_processingStates.ContainsKey(key))
            {
                _processingStates.TryGetValue(key, out result);
            }
            else
            {
                result = null;
            }
        }
        else
        {
            result = _processingStates.GetOrAdd(key, _ => new());
        }

        return result;
    }
    #endregion
    #region State
    private ConcurrentDictionary<string, AggregateSubscriptionState> _aggregateStates = new();

    public AggregateSubscriptionState GetState(AggregateDescription description) =>
        _aggregateStates.GetOrAdd(GetKey(description), _ => new());

    public void RemoveState(AggregateDescription description)
    {
        _aggregateStates.RemoveWithRetry(GetKey(description));
    }

    private static string GetKey(AggregateDescription description) =>
        $"{description.Name}_{description.Id}";
    #endregion
}