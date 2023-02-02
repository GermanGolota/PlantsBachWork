namespace Plants.Aggregates.Infrastructure;

public interface IHealthChecker
{
    Task WaitForServicesStartupOrTimeout(CancellationToken token);
}