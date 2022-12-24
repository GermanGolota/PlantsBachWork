using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Plants.Domain;
using Plants.Domain.Infrastructure.Config;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Infrastructure.Services;
using Plants.Shared;

namespace Plants.Initializer;

internal class MongoRolesDbInitializer
{
    private readonly IMongoClientFactory _factory;
    private readonly AccessesHelper _accesses;
    private readonly ConnectionConfig _connection;

    public MongoRolesDbInitializer(IMongoClientFactory factory, IOptions<ConnectionConfig> connection, AccessesHelper accesses)
    {
        _factory = factory;
        _accesses = accesses;
        _connection = connection.Value;
    }

    public async Task Initialize()
    {
        //TODO: Fix multiple envs not working
        var db = _factory.GetDatabase("admin");
        var dbName = _connection.MongoDbDatabaseName;
        await InitializeCollectionsAsync(dbName);

        await CleanUpExistingRoles(db);

        await CreateRoles(db, dbName);
    }

    private async Task InitializeCollectionsAsync(string dbName)
    {
        var envDb = _factory.GetDatabase(dbName);

        var names = await (await envDb.ListCollectionNamesAsync()).ToListAsync();
        foreach (var aggregate in _accesses.Flat.Select(x => x.Aggregate).Where(agg => names.Contains(agg) is false).Distinct())
        {
            await envDb.CreateCollectionAsync(aggregate);
        }
    }

    private static async Task CleanUpExistingRoles(IMongoDatabase db)
    {
        var existingRoles = await db.GetExistingRolesAsync();

        foreach (var role in existingRoles)
        {
            var dropRole = BsonDocument.Parse($$"""
            {
                "dropRole": "{{role}}"
            }
            """);
            var dropResult = await db.RunCommandAsync<BsonDocument>(dropRole);
        }
    }

    private async Task CreateRoles(IMongoDatabase db, string dbName)
    {
        var accessTypeToPermissions = new Dictionary<AllowType, string[]>
        {
            {AllowType.Read, new[]{"find"} },
            {AllowType.Write, new[]{"insert", "update"}}
        };

        Func<UserRole, string, string> buildPrivelege = (role, aggregate) =>
            $$"""
            {
                "resource":{ "db":"{{dbName}}", "collection":"{{aggregate}}"},
                "actions": [{{_accesses.AggregateAccesses[aggregate][role].SelectMany(type => accessTypeToPermissions[type]).QuoteDelimitList()}}]
            }
            """;

        List<string> roleDefinitions = new()
        {
            $$"""
            {
            "createRole": "changeOwnPasswordCustomDataRole",
            "privileges": [
               { 
                 "resource": { "db": "", "collection": ""},
                 "actions": [ "changeOwnPassword", "changeOwnCustomData" ]
               }
            ],
            "roles": []
            }
            """,
            $$"""
            {
            "createRole": "{{UserRole.Consumer}}",
            "privileges": [
                {{String.Join(",\n", _accesses.RoleToAggregate[UserRole.Consumer].Select(agg => buildPrivelege(UserRole.Consumer, agg)))}}
             ],
             "roles":[
                {
                    "role":"changeOwnPasswordCustomDataRole", "db":"admin" 
                }
             ]
            }
            """,
            $$"""
            {
            "createRole": "{{UserRole.Producer}}",
            "privileges": [
                {{String.Join(",\n", _accesses.RoleToAggregate[UserRole.Producer].Select(agg => buildPrivelege(UserRole.Producer, agg)))}}
             ],
             "roles":[
                {
                    "role":"changeOwnPasswordCustomDataRole", "db":"admin" 
                }]
            }
            """,
            $$"""
            {
            "createRole": "{{UserRole.Manager}}",
            "privileges": [
             ],
             "roles":["dbOwner"]
            }
            """
        };

        foreach (var definition in roleDefinitions)
        {
            var createRoleResult = await db.RunCommandAsync<BsonDocument>(BsonDocument.Parse(definition));
        }
    }
}

public static class MongoDatabaseExtensions
{
    public static async Task<IEnumerable<string>> GetExistingRolesAsync(this IMongoDatabase db)
    {
        var allRoles = Enum.GetValues<UserRole>();
        var getRolesCommand = BsonDocument.Parse($$"""
        {
            "rolesInfo": [{{allRoles.QuoteDelimitList()}}]
        }
        """);
        var rolesResult = await db.RunCommandAsync<BsonDocument>(getRolesCommand);
        return rolesResult!.GetElement("roles").Value.AsBsonArray.Select(x => x.AsBsonDocument.GetElement("role").Value.ToString());
    }

}