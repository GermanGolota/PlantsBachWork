using System.ComponentModel.DataAnnotations;

namespace Plants.Domain.Infrastructure.Config;

[ConfigSection(Section)]
public class ConnectionConfig
{
    public const string Section = "Connection";

    [Required]
    public EventStoreServiceConenction EventStore { get; set; } = null!;

    [Required]
    public MongoDbServiceConnection MongoDb { get; set; } = null!;

    [Required]
    public ServiceConnection ElasticSearch { get; set; } = null!;

    [Required]
    public ServiceCreds DefaultCreds { get; set; } = null!;

    public ServiceCreds GetCreds(Func<ConnectionConfig, ServiceConnection> connectionGetter) =>
        connectionGetter(this).Creds ?? DefaultCreds;
}

public class ServiceConnection
{
    [Required]
    public string Template { get; set; } = null!;
    public ServiceCreds? Creds { get; set; }
}

public class MongoDbServiceConnection : ServiceConnection
{
    [Required]
    public string DatabaseName { get; set; } = null!;
}

public class EventStoreServiceConenction : ServiceConnection
{
    [Range(1L, Int64.MaxValue)]
    public long TimeoutInSeconds { get; set; } = 60;
}

public record ServiceCreds([Required] string Username, [Required] string Password);