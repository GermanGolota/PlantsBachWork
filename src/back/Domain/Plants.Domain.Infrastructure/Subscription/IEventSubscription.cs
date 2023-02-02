namespace Plants.Domain.Infrastructure.Subscription;

public interface IEventSubscription
{
    Task StartAsync(CancellationToken token);
    void Stop();
}