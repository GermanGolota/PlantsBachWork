using EventStore.ClientAPI.SystemData;
using EventStore.ClientAPI.UserManagement;
using Plants.Aggregates.Infrastructure.Encryption;
using Plants.Aggregates.Services;
using Plants.Core;

namespace Plants.Aggregates.Infrastructure.Helper;

internal class EventStoreUserUpdater : IUserUpdater
{
    private readonly UsersManager _manager;
    private readonly IIdentityProvider _identity;
    private readonly SymmetricEncrypter _encrypter;

    public EventStoreUserUpdater(UsersManager manager, IIdentityProvider identity, SymmetricEncrypter encrypter)
    {
        _manager = manager;
        _identity = identity;
        _encrypter = encrypter;
    }

    public Task Create(string username, string password, string fullName, UserRole[] roles)
    {
        var groups = roles.Select(x => x.ToString()).ToArray();
        return _manager.CreateUserAsync(username, fullName, groups, password, GetCallerCreds());
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
        await _manager.UpdateUserAsync(username, fullName, groups, GetCallerCreds());
    }

    public Task UpdatePassword(string username, string oldPassword, string newPassword)
    {
        var creds = GetCallerCreds();
        return _manager.ChangePasswordAsync(username, oldPassword, newPassword, creds);
    }

    private UserCredentials GetCallerCreds()
    {
        var pass = _encrypter.Decrypt(_identity.Identity.Hash);
        return new UserCredentials(_identity.Identity.UserName, pass);
    }

}
