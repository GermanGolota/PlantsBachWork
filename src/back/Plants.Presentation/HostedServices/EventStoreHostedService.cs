namespace Plants.Presentation;

public class EventStoreHostedService : IHostedService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public EventStoreHostedService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    private IServiceScope? _scope = null;
    private IEventSubscription? _worker = null;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _scope = _scopeFactory.CreateScope();
        _worker = _scope.ServiceProvider.GetRequiredService<IEventSubscription>();
        await _worker.StartAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _worker?.Stop();
        _scope?.Dispose();

        return Task.CompletedTask;
    }
}
