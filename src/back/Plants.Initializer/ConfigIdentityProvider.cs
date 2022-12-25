using Microsoft.Extensions.Options;
using Plants.Domain;
using Plants.Domain.Services;
using Plants.Services.Infrastructure.Encryption;
using Plants.Shared;

namespace Plants.Initializer;

internal class ConfigIdentityProvider : IIdentityProvider
{
    public ConfigIdentityProvider(IOptionsSnapshot<UserConfig> options, SymmetricEncrypter encrypter)
    {
        var adminOptions = options.Get(UserConstrants.Admin);
        _identity = new UserIdentity
        {
            Roles = Enum.GetValues<UserRole>(),
            Hash = encrypter.Encrypt(adminOptions.Password),
            UserName = adminOptions.Username
        };
    }

    private readonly UserIdentity _identity;
    public IUserIdentity? Identity => _identity;

    public void UpdateIdentity(IUserIdentity newIdentity)
    {
        _identity.Roles = newIdentity.Roles;
        _identity.Hash = newIdentity.Hash;
        _identity.UserName = newIdentity.UserName;
    }

    private class UserIdentity : IUserIdentity
    {
        public UserRole[] Roles { get; set; }

        public string UserName { get; set; }

        public string Hash { get; set; }
    }

}

