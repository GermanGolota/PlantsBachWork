using Plants.Domain.Infrastructure;

namespace Plants.Presentation.HostedServices;

public class EventStoreHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EventStoreHostedService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    private IServiceScope _scope;
    private IEventSubscriptionWorker _worker;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _scope = _scopeFactory.CreateScope();
        _worker = _scope.ServiceProvider.GetRequiredService<IEventSubscriptionWorker>();
        await _worker.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _worker.Stop(cancellationToken);
        _scope.Dispose();

        return Task.CompletedTask;
    }
}
