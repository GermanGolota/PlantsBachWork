using Microsoft.Extensions.Options;
using Plants.Aggregates.Infrastructure.Encryption;
using Plants.Core;
using Plants.Domain;
using Plants.Domain.Services;

namespace Plants.Initializer;

internal class ConfigIdentityProvider : IIdentityProvider
{
    private readonly AdminUserConfig _options;
    private readonly SymmetricEncrypter _encrypter;

    public ConfigIdentityProvider(IOptions<AdminUserConfig> options, SymmetricEncrypter encrypter)
    {
        _options = options.Value;
        _encrypter = encrypter;
    }

    public IUserIdentity Identity => new UserIdentity(Enum.GetValues<UserRole>(), _options.Username, _encrypter.Encrypt(_options.Password));

    private record UserIdentity(UserRole[] Roles, string UserName, string Hash) : IUserIdentity;
}

