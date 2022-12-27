using Microsoft.Extensions.Options;
using Plants.Aggregates.Infrastructure.Helper.ElasticSearch;
using Plants.Aggregates.Services;
using Plants.Domain.Infrastructure.Extensions;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Services.Infrastructure.Encryption;
using System.Data;
using System.Net.Http.Json;

namespace Plants.Initializer;

internal class ElasticSearchRolesInitializer
{
    private readonly ElasticSearchHelper _helper;
    private readonly AccessesHelper _accesses;
    private readonly IIdentityProvider _identity;
    private readonly IIdentityHelper _identityHelper;
    private readonly SymmetricEncrypter _encrypter;
    private UserConfig _options;

    public ElasticSearchRolesInitializer(ElasticSearchHelper helper, AccessesHelper accesses, IIdentityProvider identity,
        IIdentityHelper identityHelper, SymmetricEncrypter encrypter, IOptionsSnapshot<UserConfig> options)
    {
        _helper = helper;
        _accesses = accesses;
        _identity = identity;
        _identityHelper = identityHelper;
        _encrypter = encrypter;
        _options = options.Get(UserConstrants.Admin);
    }

    public async Task Initialize()
    {
        var currentIdentity = _identity.Identity!;
        var oldUsername = currentIdentity.UserName;
        var password = _encrypter.Decrypt(currentIdentity.Hash);
        var elasticIdentity = _identityHelper.Build(password, "elastic", currentIdentity.Roles);
        _identity.UpdateIdentity(elasticIdentity);

        await CreateRolesAsync();

        await CreateAdminUserAsync();

        var oldIdentity = _identityHelper.Build(password, oldUsername, currentIdentity.Roles);
        _identity.UpdateIdentity(oldIdentity);
    }

    private async Task CreateRolesAsync()
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
            var result = await client.PostAsJsonAsync(uri, role);
            await _helper.HandleCreation<RoleDefinitionResult>("role", roleName, result, _ => _.Role.Created);
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

    private async Task CreateAdminUserAsync()
    {
        var client = _helper.GetClient();
        var url = _helper.GetUrl($"_security/user/{_options.Username}");
        var user = new UserCreateDefinition
        {
            Password = _options.Password,
            FullName = $"{_options.FirstName} {_options.LastName}",
            Roles = Enum.GetValues<UserRole>().Select(x => x.ToString()).ToList()
        };
        var result = await client.PostAsJsonAsync(url, user);

        await _helper.HandleCreation<CreationStatus>("user", _options.Username, result, _ => _.Created);
    }

}
