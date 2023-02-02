using MongoDB.Driver;

namespace Plants.Domain.Infrastructure;

public interface IMongoClientFactory
{
    MongoClient CreateClient();
    IMongoDatabase GetDatabase(string database);
}