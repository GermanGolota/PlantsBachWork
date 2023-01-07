using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Plants.Initializer.HealthCheck;

internal class HealthChecker
{
    private readonly HealthCheckService _health;
    private readonly ILogger<HealthChecker> _logger;
    private readonly HealthCheckConfig _options;

    public HealthChecker(HealthCheckService health, IOptions<HealthCheckConfig> options, ILogger<HealthChecker> logger)
    {
        _health = health;
        _logger = logger;
        _options = options.Value;
    }

    public async Task WaitForServicesStartupOrTimeout(CancellationToken token)
    {
        var timer = Stopwatch.StartNew();
        _logger.LogInformation("Waiting for services to startup for up to '{sec}' seconds", _options.TimeoutInSeconds);

        var timeoutTask = Task.Delay(TimeSpan.FromSeconds(_options.TimeoutInSeconds), token);
        var healthCheckTask = Task.Run(async () =>
        {
            while (true)
            {
                var report = await _health.CheckHealthAsync(token);
                if (report.Status == HealthStatus.Healthy || report.Status == HealthStatus.Degraded && _options.AcceptDegraded)
                {
                    break;
                }
                else
                {
                    _logger.LogInformation("Health check returned status '{status}'", report.Status);
                    foreach (var (source, entry) in report.Entries)
                    {
                        _logger.LogInformation("'{source}': '{status}' stating '{desc}'", source, entry.Status, entry.Description);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(_options.PollIntervalInSeconds));
                }
            }
        }, token);
        var resultingTask = await Task.WhenAny(timeoutTask, healthCheckTask);
        if (resultingTask == timeoutTask)
        {
            _logger.LogError("Timeout out after '{sec}' seconds while waiting for services to become available", _options.TimeoutInSeconds);
            throw new Exception("Some services failed to come up on time");
        }
        else
        {
            timer.Stop();
            _logger.LogInformation("Services sucessfully started in '{time}'!", timer.Elapsed);
        }
    }
}
