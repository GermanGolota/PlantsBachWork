using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Plants.Domain.Infrastructure;

internal class EventSubscriptionState : ISubscriptionProcessingMarker, ISubscriptionProcessingNotificator, IEventSubscriptionState
{
    public EventSubscriptionState(ILogger<EventSubscriptionState> logger)
    {
        _logger = logger;
    }

    private readonly ILogger<EventSubscriptionState> _logger;

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
            _isNew is false && _subscriptionProcessed >= _subscriptionToProcess;
    }

    private ConcurrentDictionary<string, SubscriptionProcessingState> _processingStates = new();

    public void SubscribeToNotifications(AggregateDescription description)
    {
        var state = GetProcessing(description, shouldCreate: true);
        _logger.LogDebug("Subscribed to '{@aggregate}'", description);
    }

    public bool WasProcessed(AggregateDescription description)
    {
        _logger.LogDebug("Checking subscription to '{@aggregate}'", description);
        var state = GetProcessing(description);
        return state?.WasProcessed() ?? false;
    }

    public void UnsubscribeFromNotifications(AggregateDescription description)
    {
        _logger.LogDebug("Unsubscribed to '{@aggregate}'", description);
        _processingStates.RemoveWithRetry(GetKey(description));
    }

    public void MarkSubscriptionComplete(AggregateDescription description)
    {
        _logger.LogDebug("Marking complete for '{@aggregate}'", description);
        var state = GetProcessing(description);
        state?.MarkProcessed();
    }

    public void MarkSubscribersCount(AggregateDescription description, long subscriptionsCount)
    {
        _logger.LogDebug("Marking subscribers for '{@aggregate}' - '{count}'", description, subscriptionsCount);
        var state = GetProcessing(description);
        state?.MarkSubscriptions(subscriptionsCount);
    }

    private SubscriptionProcessingState? GetProcessing(AggregateDescription description, bool shouldCreate = false)
    {
        SubscriptionProcessingState? result;

        var key = GetKey(description);
        if (shouldCreate is false)
        {
            result = _processingStates.GetValueOrDefault(key);
            if (result is null)
            {
                _logger.LogWarning("Failed to get processing state for '{@aggregate}'", description);
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
        $"{description.Name}_{description.Id}".ToLower();
    #endregion
}