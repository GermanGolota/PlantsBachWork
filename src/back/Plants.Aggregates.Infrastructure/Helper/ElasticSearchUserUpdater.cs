using Plants.Aggregates.Abstractions;
using Plants.Aggregates.Infrastructure.Helper.ElasticSearch;
using Plants.Domain.Identity;
using Plants.Shared.Model;
using System.Data;
using System.Net.Http.Json;

namespace Plants.Aggregates.Infrastructure.Helper;

public class ElasticSearchUserUpdater : IUserUpdater
{
    private readonly ElasticSearchHelper _helper;
    private readonly IIdentityProvider _identity;

    public ElasticSearchUserUpdater(ElasticSearchHelper helper, IIdentityProvider identity)
    {
        _helper = helper;
        _identity = identity;
    }

    public async Task CreateAsync(string username, string password, string fullName, UserRole[] roles, CancellationToken token = default)
    {
        var client = _helper.GetClient();
        var url = _helper.GetUrl($"_security/user/{username}");
        var user = new UserCreateDefinition
        {
            Password = password,
            FullName = fullName,
            Roles = roles.Select(x => x.ToString()).ToList()
        };
        var result = await client.PostAsJsonAsync(url, user, token);

        await _helper.HandleCreationAsync<CreationStatus>("user", username, result, _ => _.Created, token);
    }

    public async Task ChangeRoleAsync(string username, string fullName, UserRole[] oldRoles, UserRole changedRole, CancellationToken token = default)
    {
        var client = _helper.GetClient();
        var url = _helper.GetUrl($"_security/user/{username}");
        var roles =
          changedRole.ApplyChangeInto(oldRoles)
          .Select(_ => _.ToString())
          .ToList();
        var user = new UserUpdateDefinition
        {
            FullName = fullName,
            Roles = roles
        };
        var result = await client.PutAsJsonAsync(url, user, token);

        await _helper.HandleCreationAsync<CreationStatus>("user", username, result, _ => _.Created is false, token);
    }

    public async Task UpdatePasswordAsync(string username, string oldPassword, string newPassword, CancellationToken token = default)
    {
        var client = _helper.GetClient();
        var url = _identity.Identity!.UserName == username 
            ? _helper.GetUrl("/_security/user/_password")
            : _helper.GetUrl($"/_security/user/{username}/_password");
        var result = await client.PostAsJsonAsync(url, new
        {
            password = newPassword
        }, token);
        await _helper.HandleCreationAsync<object>("password change", username, result, _ => _ is not null, token);
    }

}
