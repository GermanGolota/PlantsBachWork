namespace Plants.Aggregates.Infrastructure;

public interface IBlobStoragesInitializer
{
    Task Initialize(CancellationToken token);
}