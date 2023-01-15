namespace Plants.Domain.Infrastructure;

public interface IEventSubscription
{
    Task StartAsync(CancellationToken token);
    void Stop();
}