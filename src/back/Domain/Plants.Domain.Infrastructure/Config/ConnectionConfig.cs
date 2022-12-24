using Plants.Shared;

namespace Plants.Domain.Infrastructure.Config;

[ConfigSection(Section)]
public class ConnectionConfig
{
    public const string Section = "Connection";
    public string EventStoreConnection { get; set; }
    public long EventStoreTimeoutInSeconds { get; set; } = 60;
    public string MongoDbConnectionTemplate { get; set; }
    public string MongoDbDatabaseName { get; set; }
}
