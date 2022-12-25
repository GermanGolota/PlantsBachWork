using EventStore.Client;
using Plants.Aggregates.Services;
using Plants.Domain.Infrastructure.Helpers;
using Plants.Domain.Infrastructure.Services;
using Plants.Services;
using Plants.Shared;

namespace Plants.Aggregates.Infrastructure.Services;

internal class Authorizer : IAuthorizer
{
    private readonly IJWTokenManager _tokenManager;
    private readonly IEventStoreUserManagementClientFactory _factory;

    public Authorizer(IJWTokenManager tokenManager, IEventStoreUserManagementClientFactory factory)
    {
        _tokenManager = tokenManager;
        _factory = factory;
    }

    //TODO: Integrate into user session aggregate?
    public async Task<AuthorizeResult?> Authorize(string username, string password)
    {
        AuthorizeResult? result;
        try
        {
            var user = await _factory.Create().GetCurrentUserAsync(new(username, password));
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
