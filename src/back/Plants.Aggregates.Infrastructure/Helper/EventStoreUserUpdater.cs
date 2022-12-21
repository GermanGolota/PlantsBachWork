using EventStore.Client;
using Plants.Aggregates.Services;
using Plants.Services.Infrastructure.Encryption;
using Plants.Shared;

namespace Plants.Aggregates.Infrastructure.Helper;

internal class EventStoreUserUpdater : IUserUpdater
{
    private readonly EventStoreUserManagementClient _manager;
    private readonly IIdentityProvider _identity;
    private readonly SymmetricEncrypter _encrypter;

    public EventStoreUserUpdater(EventStoreUserManagementClient manager, IIdentityProvider identity, SymmetricEncrypter encrypter)
    {
        _manager = manager;
        _identity = identity;
        _encrypter = encrypter;
    }

    public Task Create(string username, string password, string fullName, UserRole[] roles)
    {
        var groups = roles.Select(x => x.ToString()).ToArray();
        return _manager.CreateUserAsync(username, fullName, groups, password, userCredentials: GetCallerCreds());
    }

    public async Task ChangeRole(string username, string fullName, UserRole[] oldRoles, UserRole newRole)
    {
        var groups =
            (oldRoles.Contains(newRole) switch
            {
                true => oldRoles.Except(new[] { newRole }),
                false => oldRoles.Append(newRole)
            })
            .Select(_ => _.ToString())
            .ToArray();
        //TODO: Implement updateing user
        /*
        var user = await _manager.GetUserAsync(username);
        await _manager.DeleteUserAsync(username);
        var password = GetPassword();
        await Create(username, password, fullName, groups);*/
    }

    public Task UpdatePassword(string username, string oldPassword, string newPassword)
    {
        var creds = GetCallerCreds();
        return _manager.ChangePasswordAsync(username, oldPassword, newPassword, userCredentials: creds);
    }

    private UserCredentials GetCallerCreds()
    {
        var identity = _identity.Identity!;
        var pass = _encrypter.Decrypt(identity.Hash);
        return new UserCredentials(identity.UserName, pass);
    }

}
