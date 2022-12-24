using MongoDB.Driver;
using Plants.Domain.Projection;
using System.Linq.Expressions;

namespace Plants.Domain.Infrastructure.Projection;

public class MongoDBRepository<T> : IProjectionQueryService<T>, IProjectionRepository<T> where T : AggregateBase
{
    private readonly IMongoDatabase _mongoDatabase;

    public MongoDBRepository(IMongoDatabase mongoDatabase)
    {
        _mongoDatabase = mongoDatabase;
    }

    private string CollectionName => typeof(T).Name;

    public Task<bool> Exists(Guid id)
    {
        return _mongoDatabase.GetCollection<T>(CollectionName)
            .Find(x => x.Id == id)
            .AnyAsync();
    }

    public async Task<IEnumerable<T>> FindAllAsync(Expression<Func<T, bool>> predicate)
    {
        var cursor = await _mongoDatabase.GetCollection<T>(CollectionName)
            .FindAsync(predicate);
        return cursor.ToEnumerable();
    }

    public Task<T> GetByIdAsync(Guid id)
    {
        return _mongoDatabase.GetCollection<T>(CollectionName)
            .Find(x => x.Id == id)
            .SingleAsync();
    }

    public async Task InsertAsync(T entity)
    {
        try
        {
            await _mongoDatabase.GetCollection<T>(CollectionName)
                .InsertOneAsync(entity);
        }
        catch (MongoWriteException ex)
        {
            throw new RepositoryException($"Error inserting entity {entity.Id}", ex);
        }
    }

    public async Task UpdateAsync(T entity)
    {
        try
        {
            var result = await _mongoDatabase.GetCollection<T>(CollectionName)
                .ReplaceOneAsync(x => x.Id == entity.Id, entity);

            if (result.MatchedCount != 1)
            {
                throw new RepositoryException($"Missing entity {entity.Id}");
            }
        }
        catch (MongoWriteException ex)
        {
            throw new RepositoryException($"Error updating entity {entity.Id}", ex);
        }
    }
}
