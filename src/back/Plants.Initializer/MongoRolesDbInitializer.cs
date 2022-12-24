using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Plants.Domain;
using Plants.Domain.Infrastructure.Config;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Shared;

namespace Plants.Initializer;

internal class MongoRolesDbInitializer
{
    private readonly MongoClient _client;
    private readonly AccessesHelper _accesses;
    private readonly ConnectionConfig _connection;

    public MongoRolesDbInitializer(MongoClient client, IOptions<ConnectionConfig> connection, AccessesHelper accesses)
    {
        _client = client;
        _accesses = accesses;
        _connection = connection.Value;
    }

    public async Task Initialize()
    {
        //TODO: Fix multiple envs not working
        var db = _client.GetDatabase("admin");
        var dbName = _connection.MongoDbDatabaseName;
        await InitializeCollectionsAsync(dbName);

        var accessTypeToPermissions = new Dictionary<AllowType, string[]>
        {
            {AllowType.Read, new[]{"find"} },
            {AllowType.Write, new[]{"insert", "update"}}
        };

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

        Func<UserRole, string, string> buildPrivelege = (role, aggregate) =>
            $$"""
            {
                "resource":{ "db":"{{dbName}}", "collection":"{{aggregate}}"},
                "actions": [{{_accesses.Defined[aggregate][role].SelectMany(type => accessTypeToPermissions[type]).DelimitList()}}]
            }
            """;

        Dictionary<UserRole, string> roleToDefinition = new()
        {
            {UserRole.Consumer,  $$"""
            {
            "createRole": "{{UserRole.Consumer}}",
            "privileges": [
                {
                    "resource": { "cluster" : true }, 
                    "actions": ["changeOwnPassword"]
                },
                {{String.Join(",\n", _accesses.RoleToAggregate[UserRole.Consumer].Select(agg => buildPrivelege(UserRole.Consumer, agg)))}}
             ],
             "roles":[]
            }
            """},
            {UserRole.Producer, $$"""
            {
            "createRole": "{{UserRole.Producer}}",
            "privileges": [
                {
                    "resource": { "cluster" : true }, 
                    "actions": ["changeOwnPassword"]
                },
                {{String.Join(",\n", _accesses.RoleToAggregate[UserRole.Producer].Select(agg => buildPrivelege(UserRole.Producer, agg)))}}
             ],
             "roles":[]
            }
            """ },
            {UserRole.Manager,  $$"""
            {
            "createRole": "{{UserRole.Manager}}",
            "privileges": [
             ],
             "roles":["dbOwner"]
            }
            """}
        };

        foreach (var (role, definition) in roleToDefinition)
        {
            var createRoleResult = await db.RunCommandAsync<BsonDocument>(BsonDocument.Parse(definition));
        }
    }

    private async Task InitializeCollectionsAsync(string dbName)
    {
        var envDb = _client.GetDatabase(dbName);

        var names = await (await envDb.ListCollectionNamesAsync()).ToListAsync();
        foreach (var aggregate in _accesses.Flat.Select(x => x.Aggregate).Where(agg => names.Contains(agg) is false).Distinct())
        {
            await envDb.CreateCollectionAsync(aggregate);
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
            "rolesInfo": [{{allRoles.DelimitList()}}]
        }
        """);
        var rolesResult = await db.RunCommandAsync<BsonDocument>(getRolesCommand);
        return rolesResult!.GetElement("roles").Value.AsBsonArray.Select(x => x.AsBsonDocument.GetElement("role").Value.ToString());
    }

}