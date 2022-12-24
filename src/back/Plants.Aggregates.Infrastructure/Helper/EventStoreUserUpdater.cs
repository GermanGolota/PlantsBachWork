using EventStore.Client;
using EventStore.ClientAPI;
using EventStore.ClientAPI.Common.Log;
using EventStore.ClientAPI.UserManagement;
using Microsoft.Extensions.Options;
using Plants.Aggregates.Services;
using Plants.Domain.Infrastructure.Config;
using Plants.Services.Infrastructure.Encryption;
using Plants.Shared;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Security;

namespace Plants.Aggregates.Infrastructure.Helper;

internal class EventStoreUserUpdater : IUserUpdater
{
    private readonly EventStoreUserManagementClient _manager;
    private readonly IIdentityProvider _identity;
    private readonly SymmetricEncrypter _encrypter;
    private readonly ConnectionConfig _config;

    public EventStoreUserUpdater(EventStoreUserManagementClient manager, IIdentityProvider identity, SymmetricEncrypter encrypter, IOptions<ConnectionConfig> options)
    {
        _manager = manager;
        _identity = identity;
        _encrypter = encrypter;
        _config = options.Value;
    }

    public async Task Create(string username, string password, string fullName, UserRole[] roles)
    {
        var groups = roles.Select(x => x.ToString()).Append("$admins").ToArray();
        await _manager.CreateUserAsync(username, fullName, groups, password, userCredentials: GetCallerCreds());
    }

    private static bool _attachedCallback = false;

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

        if (_attachedCallback is false)
        {

            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            _attachedCallback = true;
        }

        using (var httpClientHandler = new HttpClientHandler())
        {
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) =>
            {
                return true;
            };
            var creds = GetCallerCreds();
            var uri = new Uri(_config.EventStoreConnection.Replace("esdb", "http"));
            var hostInfo = Dns.GetHostEntry(uri.Host);
            var manager = new UsersManager(
                new ConsoleLogger(),
                new IPEndPoint(hostInfo.AddressList[0], uri.Port),
                TimeSpan.FromSeconds(10),
                true,
                httpClientHandler
            );
            var user = await manager.GetCurrentUserAsync(new(creds.Username, creds.Password));
            await manager.UpdateUserAsync(username, fullName, groups, new(creds.Username, creds.Password));
        }
    }

    public async Task UpdatePassword(string username, string oldPassword, string newPassword)
    {
        var creds = GetCallerCreds();
        await _manager.ChangePasswordAsync(username, oldPassword, newPassword, userCredentials: creds);
    }

    private UserCredentials GetCallerCreds()
    {
        var identity = _identity.Identity!;
        var pass = _encrypter.Decrypt(identity.Hash);
        return new UserCredentials(identity.UserName, pass);
    }

}
