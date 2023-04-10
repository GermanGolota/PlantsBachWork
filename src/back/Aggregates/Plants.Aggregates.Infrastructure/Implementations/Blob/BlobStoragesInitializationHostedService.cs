using Microsoft.Extensions.Hosting;

namespace Plants.Aggregates.Infrastructure;

internal sealed class BlobStoragesInitializationHostedService : IHostedService
{
    private readonly IBlobStoragesInitializer _initializer;

    public BlobStoragesInitializationHostedService(IBlobStoragesInitializer initializer) =>
        _initializer = initializer;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _initializer.Initialize(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}