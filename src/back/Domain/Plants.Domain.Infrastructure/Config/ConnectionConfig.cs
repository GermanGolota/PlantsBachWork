using System.ComponentModel.DataAnnotations;

namespace Plants.Domain.Infrastructure.Config;

[ConfigSection(Section)]
public class ConnectionConfig
{
    public const string Section = "Connection";
    [Required]
    public string EventStoreConnection { get; set; } = null!;
    [Required]
    public string EventStoreServiceUsername { get; set; } = null!;
    [Required]
    public string EventStoreServicePassword { get; set; } = null!;
    public long EventStoreTimeoutInSeconds { get; set; } = 60;
    [Required]
    public string MongoDbConnectionTemplate { get; set; } = null!;
    [Required]
    public string MongoDbDatabaseName { get; set; } = null!;
}
