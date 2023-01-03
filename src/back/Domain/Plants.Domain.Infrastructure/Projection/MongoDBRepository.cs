using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Plants.Domain.Infrastructure.Config;
using Plants.Domain.Infrastructure.Services;
using System.Linq.Expressions;

namespace Plants.Domain.Infrastructure.Projection;

public class MongoDBRepository<T> : IProjectionQueryService<T>, IProjectionRepository<T> where T : AggregateBase
{
    private readonly IMongoClientFactory _clientFactory;
    private readonly ConnectionConfig _options;

    public MongoDBRepository(IMongoClientFactory clientFactory, IOptions<ConnectionConfig> options)
    {
        _clientFactory = clientFactory;
        _options = options.Value;
    }

    private IMongoDatabase Database => _clientFactory.GetDatabase(_options.MongoDbDatabaseName);
    private string CollectionName => typeof(T).Name;

    public Task<bool> ExistsAsync(Guid id)
    {
        return Database.GetCollection<T>(CollectionName)
            .Find(x => x.Id == id)
            .AnyAsync();
    }

    public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
    {
        var cursor = await Database.GetCollection<T>(CollectionName)
            .FindAsync(predicate);
        return cursor.ToEnumerable();
    }

    public Task<T> GetByIdAsync(Guid id)
    {
        return Database.GetCollection<T>(CollectionName)
            .Find(x => x.Id == id)
            .SingleAsync();
    }

    public async Task InsertAsync(T entity)
    {
        try
        {
            await Database.GetCollection<T>(CollectionName)
                .InsertOneAsync(entity);
        }
        catch (MongoWriteException ex)
        {
            throw new RepositoryException($"Error inserting entity {entity.Id}", ex, ex.WriteError.Category == ServerErrorCategory.DuplicateKey ?  RepositoryErrorCode.AlreadyExists : RepositoryErrorCode.Other);
        }
    }

    public async Task UpdateAsync(T entity)
    {
        try
        {
            var result = await Database.GetCollection<T>(CollectionName)
                .ReplaceOneAsync(x => x.Id == entity.Id, entity);

            if (result.MatchedCount != 1)
            {
                throw new RepositoryException($"Missing entity {entity.Id}", RepositoryErrorCode.NotFound);
            }
        }
        catch (MongoWriteException ex)
        {
            throw new RepositoryException($"Error updating entity {entity.Id}", ex, RepositoryErrorCode.Other);
        }
    }
}
