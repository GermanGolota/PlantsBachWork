namespace Plants.Initializer;

[ConfigSection(Section)]
internal class HealthCheckConfig
{
    const string Section = "HealthCheck";

    public bool AcceptDegraded { get; set; }
    public long TimeoutInSeconds { get; set; }
    public long PollIntervalInSeconds { get; set; }
}
