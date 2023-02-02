using Microsoft.Extensions.Options;
using Plants.Aggregates.Abstractions;
using Plants.Aggregates.Infrastructure.Helper.ElasticSearch;
using Plants.Domain.Aggregate;
using Plants.Domain.Identity;
using Plants.Domain.Infrastructure.Extensions;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Infrastructure.Domain.Helpers;
using Plants.Services.Infrastructure.Encryption;
using Plants.Shared.Model;
using System.Data;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Plants.Initializer.Roles;

internal class ElasticSearchRolesInitializer
{
    private readonly ElasticSearchHelper _helper;
    private readonly AccessesHelper _accesses;
    private readonly IIdentityProvider _identity;
    private readonly IIdentityHelper _identityHelper;
    private readonly SymmetricEncrypter _encrypter;
    private readonly AggregateHelper _aggregate;
    private UserConfig _options;

    public ElasticSearchRolesInitializer(
        ElasticSearchHelper helper, AccessesHelper accesses, 
        IIdentityProvider identity, IIdentityHelper identityHelper, 
        SymmetricEncrypter encrypter, IOptionsSnapshot<UserConfig> options,
        AggregateHelper aggregate)
    {
        _helper = helper;
        _accesses = accesses;
        _identity = identity;
        _identityHelper = identityHelper;
        _encrypter = encrypter;
        _aggregate = aggregate;
        _options = options.Get(UserConstrants.Admin);
    }

    public async Task InitializeAsync(CancellationToken token = default)
    {
        var currentIdentity = _identity.Identity!;
        var oldUsername = currentIdentity.UserName;
        var password = _encrypter.Decrypt(currentIdentity.Hash);
        var elasticIdentity = _identityHelper.Build(password, "elastic", currentIdentity.Roles);
        _identity.UpdateIdentity(elasticIdentity);

        await InitializeKibanaUserAsync(token);

        await CreateRolesAsync(token);

        await CreateAdminUserAsync(token);

        var oldIdentity = _identityHelper.Build(password, oldUsername, currentIdentity.Roles);
        _identity.UpdateIdentity(oldIdentity);

        await CreateAggregateIndexesAsync(token);
    }

    private async Task CreateAggregateIndexesAsync(CancellationToken token)
    {
        foreach (var (aggregate, _) in _aggregate.Aggregates)
        {
            var indexName = aggregate.ToIndexName();
            var client = _helper.GetClient();
            var url = _helper.GetUrl(indexName);
            var result = await client.PutAsync(url, null, token);

            await _helper.HandleCreationAsync<IndexCreationResult>("index", 
                indexName, result, 
                _ => _.Acknowledged && _.ShardsAcknowledged, token);
        }
    }

    private class IndexCreationResult
    {
        public bool Acknowledged { get; set; }
        [JsonPropertyName("shards_acknowledged")]
        public bool ShardsAcknowledged { get; set; }
    }

    private async Task InitializeKibanaUserAsync(CancellationToken token)
    {
        var client = _helper.GetClient();
        var url = _helper.GetUrl("/_security/user/kibana_system/_password");
        var result = await client.PostAsJsonAsync(url, new
        {
            password = _options.Password
        }, token);
        await _helper.HandleCreationAsync<object>("password change", "kibana_system", result, _ => _ is not null, token);
    }

    private async Task CreateRolesAsync(CancellationToken token = default)
    {
        var client = _helper.GetClient();
        var allowToPriveleges = new Dictionary<AllowType, List<string>>()
        {
            {AllowType.Read, new() {"read"} },
            {AllowType.Write, new() {"write", "create_index" } },
        };
        var roleDefs = new List<(string Name, RoleDefinition)>();
        foreach (var role in new[]
        {
            UserRole.Consumer,
            UserRole.Producer
        })
        {
            var roleName = role.ToString();
            var indices = GetIndices(allowToPriveleges, role);
            roleDefs.Add((roleName, new RoleDefinition()
            {
                Cluster = new() { },
                Indices = indices
            }));
        }
        var managerRole = new RoleDefinition()
        {
            Cluster = new() { "all" },
            Indices = new() {
                new()
                {
                    Names = new(){"*"},
                    Privileges = new(){"all"}
                }
            }
        };
        roleDefs.Add((UserRole.Manager.ToString(), managerRole));

        foreach (var (roleName, role) in roleDefs)
        {
            var uri = _helper.GetUrl($"_security/role/{roleName}");
            var result = await client.PostAsJsonAsync(uri, role, token);
            await _helper.HandleCreationAsync<RoleDefinitionResult>("role", roleName, result, _ => _.Role.Created, token);
        }
    }

    private List<RoleDefinitionIndices> GetIndices(Dictionary<AllowType, List<string>> allowToPriveleges, UserRole role)
    {
        var indices = new List<RoleDefinitionIndices>();
        foreach (var aggregate in _accesses.RoleToAggregate[role])
        {
            var priveleges = _accesses.AggregateAccesses[aggregate][role]
                .SelectMany(allow => allowToPriveleges[allow])
                .Distinct()
                .ToList();
            indices.Add(new()
            {
                Names = new() { aggregate.ToIndexName() },
                Privileges = priveleges
            });
        }

        return indices;
    }

    private async Task CreateAdminUserAsync(CancellationToken token = default)
    {
        var client = _helper.GetClient();
        var url = _helper.GetUrl($"_security/user/{_options.Username}");
        var user = new UserCreateDefinition
        {
            Password = _options.Password,
            FullName = $"{_options.FirstName} {_options.LastName}",
            Roles = Enum.GetValues<UserRole>().Select(x => x.ToString()).ToList()
        };
        var result = await client.PostAsJsonAsync(url, user, token);

        await _helper.HandleCreationAsync<CreationStatus>("user", _options.Username, result, _ => _.Created, token);
    }

}
