using System.ComponentModel.DataAnnotations;

namespace Plants.Domain.Infrastructure;

[ConfigSection(Section)]
public class ConnectionConfig
{
    public const string Section = "Connection";

    [Required]
    public EventStoreServiceConnection EventStore { get; set; } = null!;

    [Required]
    public MongoDbServiceConnection MongoDb { get; set; } = null!;

    [Required]
    public ElasticSearchConnection ElasticSearch { get; set; } = null!;

    [Required]
    public BlobServiceConnection Blob { get; set; } = null!;

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

public class BlobServiceConnection : ServiceConnection
{
    [Required]
    public string ContainerName { get; set; } = "images";
}

public class MongoDbServiceConnection : ServiceConnection
{
    [Required]
    public string DatabaseName { get; set; } = null!;
}

public class EventStoreServiceConnection : ServiceConnection
{
    [Range(1L, long.MaxValue)]
    public long TimeoutInSeconds { get; set; } = 60;
}

public class ElasticSearchConnection : ServiceConnection
{
    [Range(1L, long.MaxValue)]
    public long TimeoutInSeconds { get; set; } = 60 * 10;
}

public record ServiceCreds([Required] string Username, [Required] string Password);