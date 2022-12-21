using Microsoft.Extensions.Options;
using Plants.Aggregates.Infrastructure.Encryption;
using Plants.Domain;
using Plants.Domain.Services;
using Plants.Shared;

namespace Plants.Initializer;

internal class ConfigIdentityProvider : IIdentityProvider
{
    private readonly UserConfig _options;
    private readonly SymmetricEncrypter _encrypter;

    public ConfigIdentityProvider(IOptionsSnapshot<UserConfig> options, SymmetricEncrypter encrypter)
    {
        _options = options.Get(UserConstrants.Admin);
        _encrypter = encrypter;
    }

    public IUserIdentity Identity => new UserIdentity(Enum.GetValues<UserRole>(), _options.Username, _encrypter.Encrypt(_options.Password));

    private record UserIdentity(UserRole[] Roles, string UserName, string Hash) : IUserIdentity;
}

