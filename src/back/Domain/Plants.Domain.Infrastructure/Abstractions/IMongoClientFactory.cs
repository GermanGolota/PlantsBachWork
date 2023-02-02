using MongoDB.Driver;

namespace Plants.Domain.Infrastructure.Services;

public interface IMongoClientFactory
{
    MongoClient CreateClient();
    IMongoDatabase GetDatabase(string database);
}