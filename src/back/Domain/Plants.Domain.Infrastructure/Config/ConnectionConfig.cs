using Plants.Shared;

namespace Plants.Domain.Infrastructure.Config;

[ConfigSection(Section)]
public class ConnectionConfig
{
    public const string Section = "Connection";
    public string EventStoreConnection { get; set; }
    public long EventStoreTimeoutInSeconds { get; set; } = 60;
    //TODO: Change to template
    public string MongoDbConnection { get; set; }
    public string MongoDbDatabaseName { get; set; }
}
