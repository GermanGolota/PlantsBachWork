using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Linq.Expressions;

namespace Plants.Domain.Infrastructure;

public class MongoDBRepository<T> : IProjectionQueryService<T>, IProjectionRepository<T> where T : AggregateBase
{
    private readonly IMongoClientFactory _clientFactory;
    private readonly ConnectionConfig _options;

    public MongoDBRepository(IMongoClientFactory clientFactory, IOptions<ConnectionConfig> options)
    {
        _clientFactory = clientFactory;
        _options = options.Value;
    }

    private IMongoDatabase Database => _clientFactory.GetDatabase(_options.MongoDb.DatabaseName);
    private string CollectionName => typeof(T).Name;

    public Task<bool> ExistsAsync(Guid id, CancellationToken token = default)
    {
        return Database.GetCollection<T>(CollectionName)
            .Find(x => x.Id == id)
            .AnyAsync(cancellationToken: token);
    }

    public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate, CancellationToken token = default)
    {
        var cursor = await Database.GetCollection<T>(CollectionName)
            .FindAsync(predicate, cancellationToken: token);
        return cursor.ToEnumerable();
    }

    public Task<T> GetByIdAsync(Guid id, CancellationToken token = default)
    {
        return Database.GetCollection<T>(CollectionName)
            .Find(x => x.Id == id)
            .SingleAsync(cancellationToken: token);
    }

    public async Task InsertAsync(T entity, CancellationToken token = default)
    {
        try
        {
            await Database.GetCollection<T>(CollectionName)
                .InsertOneAsync(entity, new(), token);
        }
        catch (MongoWriteException ex)
        {
            throw new RepositoryException($"Error inserting entity {entity.Id}", ex, ex.WriteError.Category == ServerErrorCategory.DuplicateKey ?  RepositoryErrorCode.AlreadyExists : RepositoryErrorCode.Other);
        }
    }

    public async Task UpdateAsync(T entity, CancellationToken token = default)
    {
        try
        {
            var result = await Database.GetCollection<T>(CollectionName)
                .ReplaceOneAsync(x => x.Id == entity.Id, entity, cancellationToken: token);

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
