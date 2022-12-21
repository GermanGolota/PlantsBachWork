using EventStore.Client;
using Plants.Aggregates.Services;
using Plants.Services;
using Plants.Shared;

namespace Plants.Aggregates.Infrastructure.Services;

internal class Authorizer : IAuthorizer
{
    private readonly IJWTokenManager _tokenManager;
    private readonly EventStoreUserManagementClient _managementClient;

    public Authorizer(IJWTokenManager tokenManager, EventStoreUserManagementClient managementClient)
    {
        _tokenManager = tokenManager;
        _managementClient = managementClient;
    }

    //TODO: Integrate into user session aggregate?
    public async Task<AuthorizeResult?> Authorize(string username, string password)
    {
        AuthorizeResult? result;
        try
        {
            var user = await _managementClient.GetCurrentUserAsync(new(username, password));
            var roles = user.Groups
                .Select(group => (valid: Enum.TryParse<UserRole>(group, out var role), role))
                .Where(x => x.valid)
                .Select(x => x.role)
                .Distinct()
                .ToArray();
            var token = _tokenManager.CreateToken(username, password, roles);
            result = new(username, roles, token);
        }
        catch
        {
            result = null;
        }
        return result;
    }
}
