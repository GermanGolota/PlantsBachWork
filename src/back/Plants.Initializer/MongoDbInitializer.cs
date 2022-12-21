using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Plants.Domain;
using Plants.Domain.Infrastructure.Config;
using Plants.Initializer;
using Plants.Shared;

namespace Plants.Initializer;

internal class MongoDbInitializer
{
    private readonly MongoClient _client;
    private readonly ConnectionConfig _connection;

    public MongoDbInitializer(MongoClient client, IOptions<ConnectionConfig> connection)
    {
        _client = client;
        _connection = connection.Value;
    }

    public async Task Initialize(AccessorsDefinition definiton)
    {
        //TODO: Fix multiple envs not working
        var db = _client.GetDatabase("admin");
        var dbName = _connection.MongoDbDatabaseName;
        await InitializeCollectionsAsync(definiton, dbName);

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
                "actions": [{{definiton.Defined[aggregate][role].SelectMany(type => accessTypeToPermissions[type]).DelimitList()}}]
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
                {{String.Join(",\n", definiton.RoleToAggregate[UserRole.Consumer].Select(agg => buildPrivelege(UserRole.Consumer, agg)))}}
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
                {{String.Join(",\n", definiton.RoleToAggregate[UserRole.Producer].Select(agg => buildPrivelege(UserRole.Producer, agg)))}}
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

    private async Task InitializeCollectionsAsync(AccessorsDefinition definiton, string dbName)
    {
        var envDb = _client.GetDatabase(dbName);

        var names = await (await envDb.ListCollectionNamesAsync()).ToListAsync();
        foreach (var aggregate in definiton.Flat.Select(x => x.Aggregate).Where(agg => names.Contains(agg) is false).Distinct())
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