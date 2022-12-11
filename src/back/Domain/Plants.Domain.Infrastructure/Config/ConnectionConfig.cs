using Plants.Shared;

namespace Plants.Infrastructure.Config;

[ConfigSection(Section)]
public class ConnectionConfig
{
    public const string Section = "Connection";
    //TODO: Change to template
    public string EventStoreConnection { get; set; }
    //TODO: Change to template
    public string MongoDbConnection { get; set; }
    public string MongoDbDatabaseName { get; set; }
}
