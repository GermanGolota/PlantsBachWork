namespace Plants.Infrastructure.Config;

public class ConnectionConfig
{
    public string EventStoreConnection { get; set; }
    public string MongoDbConnection { get; set; }
    public string MongoDbDatabaseName { get; set; }
}
