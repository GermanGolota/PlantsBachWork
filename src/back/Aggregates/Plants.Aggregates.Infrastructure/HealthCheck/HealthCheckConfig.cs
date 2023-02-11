using System.ComponentModel.DataAnnotations;

namespace Plants.Aggregates.Infrastructure;

[ConfigSection(Section)]
internal class HealthCheckConfig
{
    const string Section = "HealthCheck";

    public bool AcceptDegraded { get; set; }
    [Range(1, long.MaxValue)]
    public long TimeoutInSeconds { get; set; }
    public long PollIntervalInSeconds { get; set; }
}
