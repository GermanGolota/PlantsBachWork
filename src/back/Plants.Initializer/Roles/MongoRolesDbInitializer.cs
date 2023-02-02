using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Plants.Initializer;

internal class MongoRolesDbInitializer
{
    private const string _commonRoleName = "changeOwnPasswordCustomDataRole";
    private readonly IMongoClientFactory _factory;
    private readonly AccessesHelper _accesses;
    private readonly ConnectionConfig _connection;

    public MongoRolesDbInitializer(IMongoClientFactory factory, IOptions<ConnectionConfig> connection, AccessesHelper accesses)
    {
        _factory = factory;
        _accesses = accesses;
        _connection = connection.Value;
    }

    public async Task InitializeAsync(CancellationToken token = default)
    {
        //TODO: Fix multiple envs not working
        var db = _factory.GetDatabase("admin");
        var dbName = _connection.MongoDb.DatabaseName;
        await InitializeCollectionsAsync(dbName, token);

        await CleanUpExistingRolesAsync(db, token);

        await CreateRolesAsync(db, dbName, token);
    }

    private async Task InitializeCollectionsAsync(string dbName, CancellationToken token = default)
    {
        var envDb = _factory.GetDatabase(dbName);

        var names = await (await envDb.ListCollectionNamesAsync(cancellationToken: token)).ToListAsync(cancellationToken: token);
        foreach (var aggregate in _accesses.Flat.Select(x => x.Aggregate).Where(agg => names.Contains(agg) is false).Distinct())
        {
            await envDb.CreateCollectionAsync(aggregate, cancellationToken: token);
        }
    }

    private static async Task CleanUpExistingRolesAsync(IMongoDatabase db, CancellationToken token = default)
    {
        var existingRoles = await GetExistingRolesAsync(db, token);

        foreach (var role in existingRoles)
        {
            var dropRole = BsonDocument.Parse($$"""
            {
                "dropRole": "{{role}}"
            }
            """);
            var dropResult = await db.RunCommandAsync<BsonDocument>(dropRole, cancellationToken: token);
        }
    }

    private async Task CreateRolesAsync(IMongoDatabase db, string dbName, CancellationToken token = default)
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

        Func<UserRole, string> buildPrivelegesString = role => string.Join(",\n", _accesses.RoleToAggregate[role].Select(agg => buildPrivelege(role, agg)));

        List<string> roleDefinitions = new()
        {
            $$"""
            {
            "createRole": "{{_commonRoleName}}",
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
                {{buildPrivelegesString(UserRole.Consumer)}}
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
                {{buildPrivelegesString(UserRole.Producer)}}
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
                {{buildPrivelegesString(UserRole.Manager)}}
             ],
             "roles":["dbOwner"]
            }
            """
        };

        foreach (var definition in roleDefinitions)
        {
            var createRoleResult = await db.RunCommandAsync<BsonDocument>(BsonDocument.Parse(definition), cancellationToken: token);
        }
    }

    private static async Task<IEnumerable<string>> GetExistingRolesAsync(IMongoDatabase db, CancellationToken token = default)
    {
        var allRoles = Enum.GetValues<UserRole>().Select(x => x.ToString()).Append(_commonRoleName);
        var getRolesCommand = BsonDocument.Parse($$"""
        {
            "rolesInfo": [{{allRoles.QuoteDelimitList()}}]
        }
        """);
        var rolesResult = await db.RunCommandAsync<BsonDocument>(getRolesCommand, cancellationToken: token);
        return rolesResult!.GetElement("roles").Value.AsBsonArray.Select(x => x.AsBsonDocument.GetElement("role").Value.ToString());
    }
}