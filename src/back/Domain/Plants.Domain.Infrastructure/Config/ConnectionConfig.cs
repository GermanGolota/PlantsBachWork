using Plants.Shared;

namespace Plants.Infrastructure.Config;

[ConfigSection(Section)]
public class ConnectionConfig
{
    public const string Section = "Connection";
    public string EventStoreConnection { get; set; }
    public string MongoDbConnection { get; set; }
    public string MongoDbDatabaseName { get; set; }
}
