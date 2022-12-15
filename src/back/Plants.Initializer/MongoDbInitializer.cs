using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using Plants.Core;
using Plants.Domain;
using Plants.Infrastructure.Config;

namespace Plants.Initializer;

public class MongoDbInitializer
{
    private readonly MongoClient _client;
    private readonly ConnectionConfig _connection;

    public MongoDbInitializer(MongoClient client, IOptions<ConnectionConfig> connection)
    {
        _client = client;
        _connection = connection.Value;
    }

    public async Task CreateRoles(AccessorsDefinition definiton)
    {

        //TODO: Fix multiple envs not working
        var db = _client.GetDatabase("admin");
        var dbName = _connection.MongoDbDatabaseName;
        var envDb = _client.GetDatabase(dbName);

        var names = await (await envDb.ListCollectionNamesAsync()).ToListAsync();
        foreach (var aggregate in definiton.Flat.Select(x => x.Aggregate).Where(agg => names.Contains(agg) is false).Distinct())
        {
            await envDb.CreateCollectionAsync(aggregate);
        }

        var allRoles = Enum.GetValues<UserRole>();
        var allRoleNames = allRoles
            .Select(x => x.ToString())
            .ToArray();

        var getRolesCommand = BsonDocument.Parse($$"""
    {
        "rolesInfo": [{{allRoles.DelimitList()}}]
    }
    """);

        var accessTypeToPermissions = new Dictionary<AllowType, string[]>
{
    {AllowType.Read, new[]{"find"} },
    {AllowType.Write, new[]{"insert", "update"}}
};

        var rolesResult = await db.RunCommandAsync<BsonDocument>(getRolesCommand);
        var existingRoles = rolesResult.GetElement("roles").Value.AsBsonArray.Select(x => x.AsBsonDocument.GetElement("role").Value.ToString());

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
}
