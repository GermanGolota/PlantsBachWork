namespace Plants.Files.Infrastructure;

public interface IBlobStoragesInitializer
{
    Task Initialize(CancellationToken token);
}