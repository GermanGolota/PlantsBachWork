﻿using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

namespace Plants.Aggregates.Infrastructure;

internal class HealthChecker : IHealthChecker
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

        var executed = await WaitForHealthCheckAsync(token).ExecuteWithTimeoutAsync(TimeSpan.FromSeconds(_options.TimeoutInSeconds), token);
        if (executed)
        {
            timer.Stop();
            _logger.LogInformation("Services sucessfully started in '{time}'!", timer.Elapsed);
        }
        else
        {
            _logger.LogError("Timeout out after '{sec}' seconds while waiting for services to become available", _options.TimeoutInSeconds);
            throw new Exception("Some services failed to come up on time");
        }
    }

    private async Task WaitForHealthCheckAsync(CancellationToken token)
    {
        while (token.IsCancellationRequested is false)
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
    }
}
