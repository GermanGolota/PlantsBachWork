using EventStore.Client;

namespace Plants.Aggregates.Infrastructure;

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
    public async Task<AuthorizeResult?> AuthorizeAsync(string username, string password, CancellationToken token = default)
    {
        AuthorizeResult? result;
        try
        {
            var user = await _factory.Create().GetCurrentUserAsync(new(username, password), cancellationToken: token);
            var roles = user.Groups
                .Select(group => (valid: Enum.TryParse<UserRole>(group, out var role), role))
                .Where(x => x.valid)
                .Select(x => x.role)
                .Distinct()
                .ToArray();
            var userToken = _tokenManager.CreateToken(username, password, roles);
            result = new(username, roles, userToken);
        }
        catch
        {
            result = null;
        }
        return result;
    }
}
