namespace Plants.Aggregates.Infrastructure.HealthCheck;

public interface IHealthChecker
{
    Task WaitForServicesStartupOrTimeout(CancellationToken token);
}