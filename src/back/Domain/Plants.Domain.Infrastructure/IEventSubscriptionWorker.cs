namespace Plants.Domain.Infrastructure;

public interface IEventSubscriptionWorker
{
    Task StartAsync(CancellationToken token);
    void Stop(CancellationToken token);
}